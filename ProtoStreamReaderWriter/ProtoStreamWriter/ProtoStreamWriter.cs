using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WojciechMikołajewicz.ProtoStreamReaderWriter.ProtoStreamWriterInternalModel;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter
{
	/// <summary>
	/// ProtoStream writer
	/// </summary>
	public partial class ProtoStreamWriter : IDisposable
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
		/// Position in internal buffer after last flush
		/// </summary>
		private int BufferPosAfterFlush;

		/// <summary>
		/// Stack for data of nested objects
		/// </summary>
		private NestData[] NestDatas;

		/// <summary>
		/// Index in stack for data of nested objects
		/// </summary>
		private int NestDatasIndex;

		/// <summary>
		/// String encoder
		/// </summary>
		private readonly Encoder StringEncoder;

#if NETSTANDARD2_0
		/// <summary>
		/// Internal buffer for string serializing
		/// </summary>
		private char[] CharBuffer;
#endif

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="stream">Stream to write to</param>
		/// <param name="bufferSize">Size of internal buffer</param>
		/// <param name="leaveOpen">Should stream be left open after writer dispose</param>
		public ProtoStreamWriter(Stream stream, int bufferSize = DefaultBufferSize, bool leaveOpen = false)
		{
			if(stream==null)
				throw new ArgumentNullException(nameof(stream));

			if(bufferSize<MinBufferSize)
				throw new ArgumentException($"Buffer size cannot be smaller than: {MinBufferSize} bytes", nameof(bufferSize));

			this.Stream=stream;
			this.LeaveOpen=leaveOpen;
			this.Buffer=System.Buffers.ArrayPool<byte>.Shared.Rent(bufferSize);
			this.NestDatas=new NestData[16];
			this.NestDatasIndex=-1;
			this.StringEncoder=this.StringEncoding.GetEncoder();
#if NETSTANDARD2_0
			this.CharBuffer=System.Buffers.ArrayPool<char>.Shared.Rent(512);
#endif
		}

	public async ValueTask FlushAsync(CancellationToken cancellationToken = default)
		{
			await this.FlushAsync(flushStream: true, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		protected virtual async ValueTask FlushAsync(bool flushStream, CancellationToken cancellationToken = default)
		{
			NestData nestData;
			Task tFlush = null;
			int i, headerLength, sizeLength;

			//Flush only when something was written to internal buffer after last flush
			if(this.BufferPosAfterFlush<this.BufferPos)
			{
				//Write size of open objects
				for(i=0; i<=this.NestDatasIndex; i++)
				{
					nestData = this.NestDatas[i];
					if(!Base128.TryWriteUInt32(destination: this.Buffer.AsSpan(nestData.DataStartIndex-nestData.SizeSpaceLength, nestData.SizeSpaceLength), value: (uint)(this.BufferPos-nestData.DataStartIndex), minBytesToWrite: nestData.SizeSpaceLength, written: out _))
						throw new Exception("Space left for object length is too small. Protocol error");
				}

				//Write buffer to stream
#if NETSTANDARD2_0
				await this.Stream.WriteAsync(this.Buffer, 0, this.BufferPos, cancellationToken)
					.ConfigureAwait(false);
#else
				await this.Stream.WriteAsync(this.Buffer.AsMemory(0, this.BufferPos), cancellationToken)
					.ConfigureAwait(false);
#endif
				if(flushStream)
					tFlush=this.Stream.FlushAsync(cancellationToken);

				this.BufferPos=0;

				//Rebuild nested objects headers
				for(i=0; i<=this.NestDatasIndex; i++)
				{
					//Try write field header and reserve place for object size
					if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: this.NestDatas[i].FieldHeader, written: out headerLength)
						|| this.Buffer.Length<headerLength+(sizeLength=Base128.GetRequiredBytesUInt32((uint)(this.Buffer.Length-this.BufferPos-headerLength))))
					{
						if(tFlush!=null)
							await tFlush.ConfigureAwait(false);
						throw new InternalBufferOverflowException("Cannot write field header, too many nested objects");
					}

					this.BufferPos+=headerLength+sizeLength;

					//Actualize nested object data
					this.NestDatas[i] = new NestData(fieldHeader: this.NestDatas[i].FieldHeader, sizeSpaceLength: sizeLength, dataStartIndex: this.BufferPos);
				}

				//Save position in buffer after flush
				this.BufferPosAfterFlush=this.BufferPos;

				//Wait for flush
				if(tFlush!=null)
					await tFlush.ConfigureAwait(false);
			}
		}

		protected ulong CalculateFieldHeader(int fieldNo, WireType wireType)
		{
			return (ulong)fieldNo<<3|(ulong)wireType;
		}

#region IDisposable Support
		protected virtual void Dispose(bool disposing)
		{
			byte[] buffer;
			char[] charBuffer;

			if(disposing)
			{
				try
				{
					this.FlushAsync(flushStream: false).GetAwaiter().GetResult();
				}
				finally
				{
					buffer=Interlocked.Exchange(ref this.Buffer, null);
					if(buffer!=null)
						System.Buffers.ArrayPool<byte>.Shared.Return(array: buffer, clearArray: true);

#if NETSTANDARD2_0
					charBuffer=Interlocked.Exchange(ref this.CharBuffer, null);
					if(charBuffer!=null)
						System.Buffers.ArrayPool<char>.Shared.Return(array: charBuffer, clearArray: true);
#endif

					if(!this.LeaveOpen)
						this.Stream.Dispose();
				}
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
		}
#endregion
	}
}