using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WojciechMikołajewicz.ProtoStreamReaderWriter.ProtoStreamReaderInternalModel;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter
{
	public partial class ProtoStreamReader : IDisposable
	{
		/// <summary>
		/// Minimum buffer size
		/// </summary>
		protected const int MinBufferSize = 1024;

		/// <summary>
		/// Default buffer size
		/// </summary>
		protected const int DefaultBufferSize = 16384;

		/// <summary>
		/// Default characters buffer size
		/// </summary>
		protected const int DefaultCharBufferSize = 128;

		/// <summary>
		/// String encoding
		/// </summary>
		protected Encoding StringEncoding { get => Encoding.UTF8; }

		/// <summary>
		/// Stream to write to
		/// </summary>
		protected Stream Stream { get; }

		/// <summary>
		/// Should stream be left open after serializer dispose
		/// </summary>
		protected bool LeaveOpen { get; }

		/// <summary>
		/// Internal buffer
		/// </summary>
		private byte[] Buffer;

		/// <summary>
		/// Current position in internal buffer
		/// </summary>
		private int BufferPos;

		/// <summary>
		/// Current position in internal buffer
		/// </summary>
		private int BufferPopulatedLength;

		/// <summary>
		/// Number of bytes safe to read - not to worry about read to much from <see cref="Stream"/>
		/// </summary>
		private ulong SafeToRead;

		/// <summary>
		/// By how many bytes buffer was shrinked so far
		/// </summary>
		private ulong ShrinkedBufferLength;

		/// <summary>
		/// Nested objects data
		/// </summary>
		private NestData[] NestDatas;

		/// <summary>
		/// Index of the top element in <see cref="NestDatas"/>
		/// </summary>
		private int NestDatasIndex;

		/// <summary>
		/// String decoder
		/// </summary>
		private readonly Decoder StringDecoder;

		/// <summary>
		/// Characters buffer
		/// </summary>
		private char[] CharBuffer;

		public ProtoStreamReader(Stream stream, int bufferSize = DefaultBufferSize, bool leaveOpen = false, ulong? bytesToReadExpected = null)
		{
			if(stream==null)
				throw new ArgumentNullException(nameof(stream));

			if(bufferSize<MinBufferSize)
				throw new ArgumentException($"Buffer size cannot be smaller than: {MinBufferSize} bytes", nameof(bufferSize));

			this.Stream=stream;
			this.LeaveOpen=leaveOpen;
			this.SafeToRead=bytesToReadExpected??0;
			this.Buffer=System.Buffers.ArrayPool<byte>.Shared.Rent(bufferSize);
			this.NestDatas=new NestData[16];
			this.NestDatasIndex=-1;
			this.StringDecoder=this.StringEncoding.GetDecoder();
			this.CharBuffer=System.Buffers.ArrayPool<char>.Shared.Rent(DefaultCharBufferSize);
		}

		public async ValueTask<ReadFieldHeaderResult> ReadFieldHeaderAsync(CancellationToken cancellationToken = default)
		{
			WireFieldHeaderData fieldHeader;
			ulong fieldLength = 0;
			bool endOfObject;
			bool endOfStream = false;
			int read;

			//Check is end of object
			endOfObject=0<=this.NestDatasIndex && this.NestDatas[this.NestDatasIndex].EndObjectPosition<=this.ShrinkedBufferLength+(ulong)this.BufferPos;
			if(endOfObject)
			{
				//End of object detected
				this.NestDatasIndex--;
				fieldHeader=new WireFieldHeaderData();
			}
			else
			{
				//Try read field header
				if(!WireProtocol.TryReadFieldHeader(this.Buffer.AsSpan(this.BufferPos, this.BufferPopulatedLength-this.BufferPos), fieldHeader: out fieldHeader, read: out read))
				{
					endOfStream=!await PopulateVarIntAsync(cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					//Try again read field header
					if(!WireProtocol.TryReadFieldHeader(this.Buffer.AsSpan(this.BufferPos, this.BufferPopulatedLength-this.BufferPos), fieldHeader: out fieldHeader, read: out read)
						&& !endOfStream)
						throw new Exception("Cannot read VarInt value. Protocol error");
				}

				this.BufferPos+=read;

				//If variable length then read length of the data
				if(fieldHeader.WireType==WireType.LengthDelimited)
				{
					fieldLength=await ReadFieldLengthAsync(cancellationToken: cancellationToken)
						.ConfigureAwait(false);
				}
			}

			return new ReadFieldHeaderResult(fieldHeader: fieldHeader, fieldLength: fieldLength, endOfObject: endOfObject, endOfStream: endOfStream);
		}

		protected async ValueTask<ulong> ReadFieldLengthAsync(CancellationToken cancellationToken)
		{
			ulong length;
			int read;

			if(!Base128.TryReadUInt64(source: this.Buffer.AsSpan(this.BufferPos, this.BufferPopulatedLength-this.BufferPos), value: out length, read: out read))
			{
				if(!await this.PopulateVarIntAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream. Cannot read VarInt value.");

				if(!Base128.TryReadUInt64(source: this.Buffer.AsSpan(this.BufferPos, this.BufferPopulatedLength-this.BufferPos), value: out length, read: out read))
					throw new Exception("Cannot read VarInt value. Protocol error");
			}
			this.BufferPos+=read;
			if(this.NestDatasIndex<0)
				this.SafeToRead=length;

			//Save entering object
			this.NestDatasIndex++;
			if(this.NestDatas.Length<=this.NestDatasIndex)
			{
				//NestDatas array is too small, allocate bigger
				NestData[] newNestData = new NestData[this.NestDatas.Length<<1];
				Array.Copy(this.NestDatas, newNestData, this.NestDatas.Length);
				this.NestDatas=newNestData;
			}
			
			this.NestDatas[this.NestDatasIndex]=new NestData(endObjectPosition: this.ShrinkedBufferLength+(ulong)this.BufferPos+length);

			return length;
		}

		#region Populate methods
		protected async ValueTask<bool> PopulateVarIntAsync(CancellationToken cancellationToken)
		{
			int read, deziredPopulateLength, i;

			if(0<this.BufferPos)
			{
				if(this.BufferPos<this.BufferPopulatedLength)
					Array.Copy(this.Buffer, this.BufferPos, this.Buffer, 0, this.BufferPopulatedLength-this.BufferPos);
				this.BufferPopulatedLength-=this.BufferPos;
				this.SafeToRead-=(ulong)this.BufferPos;
				this.ShrinkedBufferLength+=(ulong)this.BufferPos;
				this.BufferPos=0;
			}

			deziredPopulateLength=this.BufferPopulatedLength+10;//10 is the maximum VarInt length
			i=this.BufferPopulatedLength;
			while(true)
			{
				read=this.Buffer.Length-this.BufferPopulatedLength;
				if(this.LeaveOpen && this.SafeToRead<(ulong)read)
					read=Math.Max((int)this.SafeToRead, 1);

#if NETSTANDARD2_0
				if(0>=(read=await this.Stream.ReadAsync(this.Buffer, this.BufferPopulatedLength, read, cancellationToken: cancellationToken).ConfigureAwait(false)))
					return false;//ReadAsync returned zero bytes, so end of stream
#else
				if(0>=(read=await this.Stream.ReadAsync(this.Buffer.AsMemory(this.BufferPopulatedLength, read), cancellationToken: cancellationToken).ConfigureAwait(false)))
					return false;//ReadAsync returned zero bytes, so end of stream
#endif
				this.BufferPopulatedLength+=read;
				//If read 10 or more bytes then leave. 10 is the maximum VarInt length
				if(deziredPopulateLength<=this.BufferPopulatedLength)
					break;
				//Check byte after byte if whole VarInt was read
				for(; i<this.BufferPopulatedLength; i++)
					if(this.Buffer[i]<0x80)
						break;
			}
			return true;
		}

		protected async ValueTask<bool> PopulateFixedAsync(int length, CancellationToken cancellationToken)
		{
			int read, toRead;

			if(0<this.BufferPos)
			{
				if(this.BufferPos<this.BufferPopulatedLength)
					Array.Copy(this.Buffer, this.BufferPos, this.Buffer, 0, this.BufferPopulatedLength-this.BufferPos);
				this.BufferPopulatedLength-=this.BufferPos;
				this.SafeToRead-=(ulong)this.BufferPos;
				this.ShrinkedBufferLength+=(ulong)this.BufferPos;
				this.BufferPos=0;
			}

			toRead=this.Buffer.Length-this.BufferPopulatedLength;
			if(this.LeaveOpen && this.SafeToRead<(uint)toRead)
				toRead=Math.Max((int)this.SafeToRead, this.BufferPos+length-this.BufferPopulatedLength);

			length+=this.BufferPopulatedLength;//length = desired populated length
			do
			{

#if NETSTANDARD2_0
				if(0>=(read=await this.Stream.ReadAsync(this.Buffer, this.BufferPopulatedLength, toRead, cancellationToken: cancellationToken).ConfigureAwait(false)))
					return false;//ReadAsync returned zero bytes, so end of stream
#else
				if(0>=(read=await this.Stream.ReadAsync(this.Buffer.AsMemory(this.BufferPopulatedLength, toRead), cancellationToken: cancellationToken).ConfigureAwait(false)))
					return false;//ReadAsync returned zero bytes, so end of stream
#endif
				this.BufferPopulatedLength+=read;
				toRead-=read;
			}
			while(this.BufferPopulatedLength<length);
			return true;
		}
		#endregion
		#region IDisposable Support
		protected virtual void Dispose(bool disposing)
		{
			byte[] buffer;
			char[] charBuffer;

			if(disposing)
			{
				//Thread safe return Buffer to the pool
				buffer=Interlocked.Exchange(ref this.Buffer, null);
				if(buffer!=null)
					System.Buffers.ArrayPool<byte>.Shared.Return(array: buffer, clearArray: true);

				//Thread safe return CharBuffer to the pool
				charBuffer=Interlocked.Exchange(ref this.CharBuffer, null);
				if(charBuffer!=null)
					System.Buffers.ArrayPool<char>.Shared.Return(array: charBuffer, clearArray: true);

				if(!this.LeaveOpen)
					this.Stream.Dispose();
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
		}
		#endregion
	}
}