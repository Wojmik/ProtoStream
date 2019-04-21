using ProtoStream.InternalModel;
using System.Buffers.Binary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ProtoStream
{
	/// <summary>
	/// ProtoStream protocol reader
	/// </summary>
	public class ProtoStreamReader : IDisposable
	{
		/// <summary>
		/// Minimum buffer size
		/// </summary>
		protected const int MinBufferSize = 1024;

		/// <summary>
		/// Default buffer size
		/// </summary>
		public const int DefaultBufferSize = ushort.MaxValue+DefaultMaxDeepLevel*7;

		/// <summary>
		/// Default maximum objects nest level
		/// </summary>
		public const int DefaultMaxDeepLevel = 32;

		/// <summary>
		/// Default string buffer size in chars
		/// </summary>
		public const int DefaultStringBufferSize = 16384;

		/// <summary>
		/// Shoud internal buffer has to be celard after use (it can be reused for another purposes)
		/// </summary>
		public virtual bool ClearBufferAfterUse { get; set; }

		/// <summary>
		/// String encoding
		/// </summary>
		public Encoding StringEncoding { get => Encoding.UTF8; }

		/// <summary>
		/// Decoder used to decode strings
		/// </summary>
		public Decoder StringDecoder { get; }

		/// <summary>
		/// Current field header
		/// </summary>
		public FieldHeader CurrentFieldHeader { get; protected set; }

		/// <summary>
		/// Maximum bytes per char
		/// </summary>
		protected int MaxBytesPerChar { get; }

		/// <summary>
		/// Stream to read from
		/// </summary>
		protected Stream Stream { get; }

		/// <summary>
		/// Should stream be left open after serializer dispose
		/// </summary>
		protected bool LeaveOpen { get; }

		/// <summary>
		/// Internal buffer
		/// </summary>
		protected byte[] Buffer;

		/// <summary>
		/// Current position in internal buffer
		/// </summary>
		protected int BufferPos;

		/// <summary>
		/// Current not consumed data length in internal buffer
		/// </summary>
		protected int BufferLength;

		/// <summary>
		/// Stack of nest objects data
		/// </summary>
		protected NestDataReadStack NestDataStack;

		/// <summary>
		/// Buffer for deserializing strings
		/// </summary>
		protected char[] StringBuf;

		/// <summary>
		/// Store for user data assigned to the deserializing object
		/// </summary>
		protected readonly Dictionary<List<int>, object> UserData = new Dictionary<List<int>, object>(IntArrayEqualityComparer.Default);

		protected readonly List<int> TempUserDataKey = new List<int>();

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="stream">Stream to read from</param>
		/// <param name="bufferSize">Size of buffer</param>
		/// <param name="leaveOpen">Should stream be left open after serializer dispose</param>
		public ProtoStreamReader(Stream stream, int bufferSize = DefaultBufferSize, bool leaveOpen = false)
		{
			if(bufferSize<MinBufferSize)
				throw new ArgumentException($"Buffer size cannot be smaller than: {MinBufferSize} bytes", nameof(bufferSize));

			this.StringDecoder=StringEncoding.GetDecoder();
			this.MaxBytesPerChar=StringEncoding.GetMaxByteCount(1);
			this.Stream=stream??throw new ArgumentNullException(nameof(stream));
			this.LeaveOpen=leaveOpen;
			this.Buffer=System.Buffers.ArrayPool<byte>.Shared.Rent(bufferSize);
			this.NestDataStack=new NestDataReadStack(DefaultMaxDeepLevel);
			this.NestDataStack.Push(fieldNo: -1, nestLevelLength: int.MaxValue);
			this.StringBuf=System.Buffers.ArrayPool<char>.Shared.Rent(DefaultStringBufferSize);
		}

		public void StoreUserData(object userData)
		{
			UserData[CopyObjectId(GetCurrentObjectId())]=userData;
		}

		public bool TryGetUserData(out object userData)
		{
			return UserData.TryGetValue(GetCurrentObjectId(), out userData);
		}

		public object GetOrCreateUserData(Func<object> userData)
		{
			List<int> objectId;
			object obj;

			objectId=GetCurrentObjectId();
			if(!UserData.TryGetValue(objectId, out obj))
				UserData.Add(CopyObjectId(objectId), obj=userData());
			return obj;
		}

		protected List<int> GetCurrentObjectId()
		{
			int i;

			TempUserDataKey.Clear();
			//objectId = new int[NestDataStack.CurrentNestLevel+(CurrentFieldHeader.WireType!=Internal.WireType.LengthDelimited ? 1 : 0)];
			for(i=0; i<NestDataStack.CurrentNestLevel; i++)
				TempUserDataKey.Add(NestDataStack[i+1].FieldNo);
			if(CurrentFieldHeader.WireType!=Internal.WireType.LengthDelimited)
				TempUserDataKey.Add(CurrentFieldHeader.FieldNo);
			return TempUserDataKey;
		}

		protected List<int> CopyObjectId(List<int> objectId)
		{
			List<int> copy = new List<int>(objectId.Count);
			copy.AddRange(objectId);
			return copy;
		}

		/// <summary>
		/// Deserialize field header (field wire type, field no and field length for length delimited fields). Field no will be minus five for nested object leaving and minus ten for end of stream.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Field header (field wire type, field no and field length for length delimited fields)</returns>
		public async ValueTask<FieldHeader> ReadFieldHeaderAsync(CancellationToken cancellationToken = default)
		{
			FieldHeader fieldHeader;
			int read;
			ushort length;

			//Check is it leaving nested object situation
			if(NestDataStack.CurrentNestData.NestLevelLength<=BufferPos)
			{
				NestDataStack.Pop();//Pop object from stack
				return new FieldHeader() { FieldNo=(int)Model.FieldNoEvent.LeavingNestedObject, };//Leaving nested object
			}

			while(!Internal.WireProtocol.TryReadFieldKey(source: new ReadOnlySpan<byte>(Buffer, BufferPos, BufferLength), type: out fieldHeader.WireType, fieldNo: out fieldHeader.FieldNo, read: out read))
			{
				if(!await ReadMoreDataAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
					if(NestDataStack.CurrentNestLevel==0 && BufferLength==0)
						return new FieldHeader() { FieldNo=(int)Model.FieldNoEvent.EndOfStream, };//End of stream
					else
						throw new EndOfStreamException("Unexpected end of stream");
			}
			BufferPos+=read;
			BufferLength-=read;

			if(fieldHeader.WireType==Internal.WireType.LengthDelimited)//If length delimited object then read length
			{
				//Read new object length
				while(!BinaryPrimitives.TryReadUInt16LittleEndian(source: new ReadOnlySpan<byte>(Buffer, BufferPos, BufferLength), value: out length))
				{
					if(!await ReadMoreDataAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
						throw new EndOfStreamException("Unexpected end of stream");
				}
				BufferPos+=ProtoStreamWriter.SizeOfLength;
				BufferLength-=ProtoStreamWriter.SizeOfLength;

				fieldHeader.FieldLength=(int)length;
				//Push object to stack
				NestDataStack.Push(fieldNo: fieldHeader.FieldNo, nestLevelLength: BufferPos+fieldHeader.FieldLength);
			}
			else
			{
				fieldHeader.FieldLength=0;
				if(fieldHeader.WireType==Internal.WireType.StartGroup)
					fieldHeader.FieldNo=(int)Model.FieldNoEvent.StartGroup;
				else if(fieldHeader.WireType==Internal.WireType.EndGroup)
					fieldHeader.FieldNo=(int)Model.FieldNoEvent.EndGroup;
			}

			CurrentFieldHeader=fieldHeader;
			return fieldHeader;
		}

		/// <summary>
		/// Deserialize long value - variable length
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Deserialized value</returns>
		public async ValueTask<long> ReadVarIntAsync(CancellationToken cancellationToken = default)
		{
			long value;
			int read;

			while(!Internal.Base64VarInt.TryReadInt64VarInt(source: new ReadOnlySpan<byte>(Buffer, BufferPos, BufferLength), value: out value, read: out read))
			{
				if(!await ReadMoreDataAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream");
			}
			BufferPos+=read;
			BufferLength-=read;
			return value;
		}

		/// <summary>
		/// Deserialize long value - variable length
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Deserialized value</returns>
		public async ValueTask<ulong> ReadVarUIntAsync(CancellationToken cancellationToken = default)
		{
			ulong value;
			int read;

			while(!Internal.Base64VarInt.TryReadUInt64VarInt(source: new ReadOnlySpan<byte>(Buffer, BufferPos, BufferLength), value: out value, read: out read))
			{
				if(!await ReadMoreDataAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream");
			}
			BufferPos+=read;
			BufferLength-=read;
			return value;
		}

		/// <summary>
		/// Deserialize long value - variable length
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Deserialized value</returns>
		public async ValueTask<ValueWithSize<long>> ReadVarIntWithSizeAsync(CancellationToken cancellationToken = default)
		{
			ValueWithSize<long> varIntWithSize;

			while(!Internal.Base64VarInt.TryReadInt64VarInt(source: new ReadOnlySpan<byte>(Buffer, BufferPos, BufferLength), value: out varIntWithSize.Value, read: out varIntWithSize.Size))
			{
				if(!await ReadMoreDataAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream");
			}
			BufferPos+=varIntWithSize.Size;
			BufferLength-=varIntWithSize.Size;
			return varIntWithSize;
		}

		/// <summary>
		/// Deserialize long value - variable length
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Deserialized value</returns>
		public async ValueTask<ValueWithSize<ulong>> ReadVarUIntWithSizeAsync(CancellationToken cancellationToken = default)
		{
			ValueWithSize<ulong> varUIntWithSize;

			while(!Internal.Base64VarInt.TryReadUInt64VarInt(source: new ReadOnlySpan<byte>(Buffer, BufferPos, BufferLength), value: out varUIntWithSize.Value, read: out varUIntWithSize.Size))
			{
				if(!await ReadMoreDataAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream");
			}
			BufferPos+=varUIntWithSize.Size;
			BufferLength-=varUIntWithSize.Size;
			return varUIntWithSize;
		}

		/// <summary>
		/// Deserialize int value - fixed length
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Deserialized value</returns>
		public async ValueTask<int> ReadInt32Async(CancellationToken cancellationToken = default)
		{
			int value;

			//Read more data if buffer has insufficient data
			while(BufferLength<sizeof(int))
			{
				if(!await ReadMoreDataAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream");
			}

			value=BinaryPrimitives.ReadInt32LittleEndian(new ReadOnlySpan<byte>(Buffer, BufferPos, sizeof(int)));
			BufferPos+=sizeof(int);
			BufferLength-=sizeof(int);
			return value;
		}

		/// <summary>
		/// Deserialize int value - fixed length
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Deserialized value</returns>
		public async ValueTask<long> ReadInt64Async(CancellationToken cancellationToken = default)
		{
			long value;

			//Read more data if buffer has insufficient data
			while(BufferLength<sizeof(long))
			{
				if(!await ReadMoreDataAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream");
			}

			value=BinaryPrimitives.ReadInt64LittleEndian(new ReadOnlySpan<byte>(Buffer, BufferPos, sizeof(long)));
			BufferPos+=sizeof(long);
			BufferLength-=sizeof(long);
			return value;
		}

		/// <summary>
		/// Get current object length. For top level object it allways return int.MaxValue.
		/// </summary>
		/// <returns>Length of current object</returns>
		public int GetCurrentObjectLength()
		{
			return NestDataStack.CurrentNestData.NestLevelLength-BufferPos;
		}

		/// <summary>
		/// Read content chunk of length delimited object
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Content chunk of length delimited object</returns>
		public ReadOnlyMemory<byte> ReadLengthDelimitedObjectContentChunkAsync(CancellationToken cancellationToken = default)
		{
			return new ReadOnlyMemory<byte>(Buffer, BufferPos, Math.Min(BufferLength, NestDataStack.CurrentNestData.NestLevelLength-BufferPos));
		}

		public void MarkBytesAsRead(int read)
		{
			BufferPos+=read;
			BufferLength-=read;
		}

		/// <summary>
		/// Reads string
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Streang read</returns>
		public async ValueTask<string> ReadStringAsync(CancellationToken cancellationToken = default)
		{
			FieldHeader fieldHeader;
			int charIndex=0, bytesToRead, read, charsRead;
			bool completed, end;
			string str;

			bytesToRead=NestDataStack.CurrentNestData.NestLevelLength-BufferPos;
			//If char buffer is to small, get new one - bigger
			if(StringBuf.Length<bytesToRead)
			{
				System.Buffers.ArrayPool<char>.Shared.Return(StringBuf, this.ClearBufferAfterUse);
				StringBuf=System.Buffers.ArrayPool<char>.Shared.Rent(bytesToRead);
			}
			
			StringDecoder.Reset();
			do
			{
				bytesToRead=NestDataStack.CurrentNestData.NestLevelLength-BufferPos;
				//Read more data if buffer is almost empty
				while(BufferLength<this.MaxBytesPerChar && BufferLength<bytesToRead)
				{
					if(!await ReadMoreDataAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
						throw new EndOfStreamException("Unexpected end of stream");
				}
				end=bytesToRead<=BufferLength;
				if(!end)
					bytesToRead=BufferLength;
				StringDecoder.Convert(Buffer, BufferPos, bytesToRead, StringBuf, charIndex, StringBuf.Length-charIndex, end, out read, out charsRead, out completed);
				BufferPos+=read;
				BufferLength-=read;
				charIndex+=charsRead;
			}
			while(!end);

			fieldHeader=await ReadFieldHeaderAsync(cancellationToken: cancellationToken)
				.ConfigureAwait(false);
			if(fieldHeader.FieldNo!=(int)Model.FieldNoEvent.LeavingNestedObject)
				throw new SerializationException($"Should be leaving nested object and is: {fieldHeader.WireType}");

			str=new string(StringBuf, 0, charIndex);
			
			return str;
		}

		/// <summary>
		/// Reads bytes array
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Bytes array read</returns>
		public async ValueTask<byte[]> ReadBytesArrayAsync(CancellationToken cancellationToken = default)
		{
			FieldHeader fieldHeader;
			byte[] byteBuf;
			int bytesRead = 0, bytesToRead;


			byteBuf=new byte[NestDataStack.CurrentNestData.NestLevelLength-BufferPos];
			while(true)
			{
				bytesToRead=NestDataStack.CurrentNestData.NestLevelLength-BufferPos;
				if(BufferLength<bytesToRead)
					bytesToRead=BufferLength;

				Array.Copy(Buffer, BufferPos, byteBuf, bytesRead, bytesToRead);
				BufferPos+=bytesToRead;
				BufferLength-=bytesToRead;
				bytesRead+=bytesToRead;

				if(0<NestDataStack.CurrentNestData.NestLevelLength-BufferPos)
				{
					if(!await ReadMoreDataAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
						throw new EndOfStreamException("Unexpected end of stream");
				}
				else
					break;
			}

			fieldHeader=await ReadFieldHeaderAsync(cancellationToken: cancellationToken)
				.ConfigureAwait(false);
			if(fieldHeader.FieldNo!=(int)Model.FieldNoEvent.LeavingNestedObject)
				throw new SerializationException($"Should be leaving nested object and is: {fieldHeader.WireType}");

			return byteBuf;
		}

		/// <summary>
		/// Reads bytes array
		/// </summary>
		/// <param name="byteArray">Bytes array to read data to</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public async ValueTask ReadBytesArrayAsync(Memory<byte> byteArray, CancellationToken cancellationToken = default)
		{
			FieldHeader fieldHeader;
			int bytesRead = 0, bytesToRead;

			while(true)
			{
				bytesToRead=NestDataStack.CurrentNestData.NestLevelLength-BufferPos;
				if(BufferLength<bytesToRead)
					bytesToRead=BufferLength;

				new Memory<byte>(Buffer, BufferPos, bytesToRead).CopyTo(byteArray.Slice(bytesRead));
				BufferPos+=bytesToRead;
				BufferLength-=bytesToRead;
				bytesRead+=bytesToRead;

				if(0<NestDataStack.CurrentNestData.NestLevelLength-BufferPos)
				{
					if(!await ReadMoreDataAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
						throw new EndOfStreamException("Unexpected end of stream");
				}
				else
					break;
			}

			fieldHeader=await ReadFieldHeaderAsync(cancellationToken: cancellationToken)
				.ConfigureAwait(false);
			if(fieldHeader.FieldNo!=(int)Model.FieldNoEvent.LeavingNestedObject)
				throw new SerializationException($"Should be leaving nested object and is: {fieldHeader.WireType}");
		}

		/// <summary>
		/// Skips length delimited field
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public async ValueTask SkipLengthDelimitedFieldAsync(CancellationToken cancellationToken = default)
		{
			FieldHeader fieldHeader;

			await SkipBytesAsync(bytesToSkip: NestDataStack.CurrentNestData.NestLevelLength-BufferPos, cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			fieldHeader=await ReadFieldHeaderAsync(cancellationToken: cancellationToken)
				.ConfigureAwait(false);
			if(fieldHeader.FieldNo!=(int)Model.FieldNoEvent.LeavingNestedObject)
				throw new SerializationException($"Should be leaving nested object and is: {fieldHeader.WireType}");
		}

		/// <summary>
		/// Reads bytes array
		/// </summary>
		/// <param name="bytesToSkip">Number of bytes to skip</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public async ValueTask SkipBytesAsync(int bytesToSkip, CancellationToken cancellationToken = default)
		{
			int bytesToRead;

			while(true)
			{
				bytesToRead=bytesToSkip;
				if(BufferLength<bytesToRead)
					bytesToRead=BufferLength;

				BufferPos+=bytesToRead;
				BufferLength-=bytesToRead;
				bytesToSkip-=bytesToRead;

				if(0<bytesToSkip)
				{
					if(!await ReadMoreDataAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
						throw new EndOfStreamException("Unexpected end of stream");
				}
				else
					break;
			}
		}

		/// <summary>
		/// Reads more data to internal buffer
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>True if data read, false if end of transmission</returns>
		public async ValueTask<bool> ReadMoreDataAsync(CancellationToken cancellationToken = default)
		{
			int read, i;

			if(BufferPos>0)
			{
				//Shift not consumed part of buffer to the begining
				Array.Copy(Buffer, BufferPos, Buffer, 0, BufferLength);
				//Shrink length of all nest levels but zero
				for(i=1; i<=NestDataStack.CurrentNestLevel; i++)
					NestDataStack[i].NestLevelLength-=BufferPos;
				//Set current buffer's position to zero
				BufferPos=0;
			}

			//Read more data
#if NETCOREAPP
			read=await Stream.ReadAsync(new Memory<byte>(Buffer, BufferLength, Buffer.Length-BufferLength), cancellationToken)
				.ConfigureAwait(false);
#else
			read=await Stream.ReadAsync(Buffer, BufferLength, Buffer.Length-BufferLength, cancellationToken)
				.ConfigureAwait(false);
#endif
			BufferLength+=read;
			return read>0;
		}

#region IDisposable Support
		private int disposedValue = 0; // Aby wykryć nadmiarowe wywołania

		protected virtual void Dispose(bool disposing)
		{
			bool disposedValue;

			disposedValue=(0!=System.Threading.Interlocked.Exchange(ref this.disposedValue, 1));//Gwarancja że wnętrze metody wykona się tylko raz
			if(!disposedValue)
			{
				if(disposing)
				{
					if(!LeaveOpen)
						this.Stream.Dispose();
				}

				// TODO: Zwolnić niezarządzane zasoby (niezarządzane obiekty) i przesłonić poniższy finalizator.
				// TODO: ustaw wartość null dla dużych pól.
				if(this.Buffer!=null)
					System.Buffers.ArrayPool<byte>.Shared.Return(this.Buffer, this.ClearBufferAfterUse);//Ta funkcja koniecznie musi być wywołana, inaczej będzie następował wyciek pamięci
				this.Buffer=null;

				if(this.StringBuf!=null)
					System.Buffers.ArrayPool<char>.Shared.Return(this.StringBuf, this.ClearBufferAfterUse);
				this.StringBuf=null;

				disposedValue = true;
			}
		}

		// TODO: Przesłonić finalizator tylko w sytuacji, gdy powyższa metoda Dispose(bool disposing) zawiera kod służący do zwalniania niezarządzanych zasobów.
		~ProtoStreamReader()
		{
			// Nie zmieniaj tego kodu. Umieść kod czyszczący w powyższej metodzie Dispose(bool disposing).
			Dispose(false);
		}

		// Ten kod został dodany w celu prawidłowego zaimplementowania wzorca rozporządzającego.
		public void Dispose()
		{
			// Nie zmieniaj tego kodu. Umieść kod czyszczący w powyższej metodzie Dispose(bool disposing).
			Dispose(true);
			// TODO: Usunąć komentarz z poniższego wiersza, jeśli finalizator został przesłonięty powyżej.
			GC.SuppressFinalize(this);
		}
#endregion
	}
}