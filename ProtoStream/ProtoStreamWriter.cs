using ProtoStream.InternalModel;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoStream
{
	/// <summary>
	/// ProtoStream protocol writer
	/// </summary>
	public class ProtoStreamWriter : IDisposable
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
		/// Size of place for length
		/// </summary>
		public const int SizeOfLength = sizeof(ushort);

		/// <summary>
		/// Shoud internal buffer has to be celard after use (it can be reused for another purposes)
		/// </summary>
		public virtual bool ClearBufferAfterUse { get; set; }

		/// <summary>
		/// String encoding
		/// </summary>
		public Encoding StringEncoding { get => Encoding.UTF8; }

		/// <summary>
		/// Maximum number of bytes for one character
		/// </summary>
		public int MaxBytesPerChar { get; }

		/// <summary>
		/// Encoder used to encode strings
		/// </summary>
		public Encoder StringEncoder { get; }

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
		protected byte[] Buffer;

		/// <summary>
		/// Current position in internal buffer
		/// </summary>
		protected int BufferPos;

		/// <summary>
		/// Stack of nest objects data
		/// </summary>
		protected NestDataWriteStack NestDataStack;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="stream">Stream to write to</param>
		/// <param name="bufferSize">Size of buffer</param>
		/// <param name="leaveOpen">Should stream be left open after serializer dispose</param>
		public ProtoStreamWriter(Stream stream, int bufferSize = DefaultBufferSize, bool leaveOpen = false)
		{
			if(bufferSize<MinBufferSize)
				throw new ArgumentException($"Buffer size cannot be smaller than: {MinBufferSize} bytes", nameof(bufferSize));

			this.MaxBytesPerChar=StringEncoding.GetMaxByteCount(1);
			this.StringEncoder=StringEncoding.GetEncoder();
			this.Stream=stream??throw new ArgumentNullException(nameof(stream));
			this.LeaveOpen=leaveOpen;
			this.Buffer=System.Buffers.ArrayPool<byte>.Shared.Rent(bufferSize);
			this.NestDataStack=new NestDataWriteStack(DefaultMaxDeepLevel);
			this.NestDataStack.Push(fieldNo: -1, headerLength: 0, levelDataStartIndex: this.BufferPos, bufferAvailableLength: Buffer.Length);
		}

		/// <summary>
		/// Serialize field header (field wire type and field no)
		/// </summary>
		/// <param name="type">Field wire type</param>
		/// <param name="fieldNo">Field unique number</param>
		/// <param name="minPlaceAfterHeader">Required minimum free buffer space in bytes after field header</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Bytes written</returns>
		public async ValueTask<int> WriteFieldHeaderAsync(Internal.WireType type, int fieldNo, int minPlaceAfterHeader, CancellationToken cancellationToken = default)
		{
			int written;

			if(NestDataStack.CurrentNestData.BufferAvailableLength-BufferPos<5+minPlaceAfterHeader)
				await FlushAsync(cancellationToken: cancellationToken)
					.ConfigureAwait(false);

			Internal.WireProtocol.TryWriteFieldKey(destination: new Span<byte>(Buffer, BufferPos, 5), type: type, fieldNo: fieldNo, written: out written);
			BufferPos+=written;
			return written;
		}

		/// <summary>
		/// Serialize long field
		/// </summary>
		/// <param name="fieldNo">Field unique number</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public async ValueTask WriteVarIntAsync(int fieldNo, long value, CancellationToken cancellationToken = default)
		{
			int written;

			if(NestDataStack.CurrentNestData.BufferAvailableLength-BufferPos<15)
				await FlushAsync(cancellationToken: cancellationToken)
					.ConfigureAwait(false);

			Internal.WireProtocol.TryWriteFieldKey(destination: new Span<byte>(Buffer, BufferPos, 5), type: Internal.WireType.VarInt, fieldNo: fieldNo, written: out written);
			BufferPos+=written;

			Internal.Base64VarInt.TryWriteInt64VarInt(destination: new Span<byte>(Buffer, BufferPos, 10), value: value, written: out written);
			BufferPos+=written;
		}

		/// <summary>
		/// Serialize long value
		/// </summary>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public async ValueTask WriteVarIntValueAsync(long value, CancellationToken cancellationToken = default)
		{
			int written;

			if(NestDataStack.CurrentNestData.BufferAvailableLength-BufferPos<10)
				await FlushAsync(cancellationToken: cancellationToken)
					.ConfigureAwait(false);

			Internal.Base64VarInt.TryWriteInt64VarInt(destination: new Span<byte>(Buffer, BufferPos, 10), value: value, written: out written);
			BufferPos+=written;
		}

		/// <summary>
		/// Serialize ulong field
		/// </summary>
		/// <param name="fieldNo">Field unique number</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public async ValueTask WriteVarIntAsync(int fieldNo, ulong value, CancellationToken cancellationToken = default)
		{
			int written;

			if(NestDataStack.CurrentNestData.BufferAvailableLength-BufferPos<15)
				await FlushAsync(cancellationToken: cancellationToken)
					.ConfigureAwait(false);

			Internal.WireProtocol.TryWriteFieldKey(destination: new Span<byte>(Buffer, BufferPos, 5), type: Internal.WireType.VarInt, fieldNo: fieldNo, written: out written);
			BufferPos+=written;

			Internal.Base64VarInt.TryWriteUInt64VarInt(destination: new Span<byte>(Buffer, BufferPos, 10), value: value, written: out written);
			BufferPos+=written;
		}

		/// <summary>
		/// Serialize ulong value
		/// </summary>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public async ValueTask WriteVarIntValueAsync(ulong value, CancellationToken cancellationToken = default)
		{
			int written;

			if(NestDataStack.CurrentNestData.BufferAvailableLength-BufferPos<10)
				await FlushAsync(cancellationToken: cancellationToken)
					.ConfigureAwait(false);

			Internal.Base64VarInt.TryWriteUInt64VarInt(destination: new Span<byte>(Buffer, BufferPos, 10), value: value, written: out written);
			BufferPos+=written;
		}

		/// <summary>
		/// Serialize fixed length int field
		/// </summary>
		/// <param name="fieldNo">Field unique number</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public async ValueTask WriteInt32Async(int fieldNo, int value, CancellationToken cancellationToken = default)
		{
			int written;

			if(NestDataStack.CurrentNestData.BufferAvailableLength-BufferPos<5+sizeof(int))
				await FlushAsync(cancellationToken: cancellationToken)
					.ConfigureAwait(false);

			Internal.WireProtocol.TryWriteFieldKey(destination: new Span<byte>(Buffer, BufferPos, 5), type: Internal.WireType.Fixed32, fieldNo: fieldNo, written: out written);
			BufferPos+=written;

			BinaryPrimitives.WriteInt32LittleEndian(destination: new Span<byte>(Buffer, BufferPos, sizeof(int)), value: value);
			BufferPos+=sizeof(int);
		}

		/// <summary>
		/// Serialize fixed length int value
		/// </summary>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public async ValueTask WriteInt32ValueAsync(int value, CancellationToken cancellationToken = default)
		{
			if(NestDataStack.CurrentNestData.BufferAvailableLength-BufferPos<sizeof(int))
				await FlushAsync(cancellationToken: cancellationToken)
					.ConfigureAwait(false);

			BinaryPrimitives.WriteInt32LittleEndian(destination: new Span<byte>(Buffer, BufferPos, sizeof(int)), value: value);
			BufferPos+=sizeof(int);
		}

		/// <summary>
		/// Serialize fixed length long field
		/// </summary>
		/// <param name="fieldNo">Field unique number</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public async ValueTask WriteInt64Async(int fieldNo, long value, CancellationToken cancellationToken = default)
		{
			int written;

			if(NestDataStack.CurrentNestData.BufferAvailableLength-BufferPos<5+sizeof(long))
				await FlushAsync(cancellationToken: cancellationToken)
					.ConfigureAwait(false);

			Internal.WireProtocol.TryWriteFieldKey(destination: new Span<byte>(Buffer, BufferPos, 5), type: Internal.WireType.Fixed64, fieldNo: fieldNo, written: out written);
			BufferPos+=written;

			BinaryPrimitives.WriteInt64LittleEndian(destination: new Span<byte>(Buffer, BufferPos, sizeof(long)), value: value);
			BufferPos+=sizeof(long);
		}

		/// <summary>
		/// Serialize fixed length long value
		/// </summary>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public async ValueTask WriteInt64ValueAsync(long value, CancellationToken cancellationToken = default)
		{
			if(NestDataStack.CurrentNestData.BufferAvailableLength-BufferPos<sizeof(long))
				await FlushAsync(cancellationToken: cancellationToken)
					.ConfigureAwait(false);

			BinaryPrimitives.WriteInt64LittleEndian(destination: new Span<byte>(Buffer, BufferPos, sizeof(long)), value: value);
			BufferPos+=sizeof(long);
		}

		/// <summary>
		/// Serialize bool value
		/// </summary>
		/// <param name="fieldNo">Field unique number</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public async ValueTask WriteBoolAsync(int fieldNo, bool value, CancellationToken cancellationToken = default)
		{
			int written;

			if(NestDataStack.CurrentNestData.BufferAvailableLength-BufferPos<5+sizeof(bool))
				await FlushAsync(cancellationToken: cancellationToken)
					.ConfigureAwait(false);

			Internal.WireProtocol.TryWriteFieldKey(destination: new Span<byte>(Buffer, BufferPos, 5+sizeof(bool)), type: Internal.WireType.VarInt, fieldNo: fieldNo, written: out written);
			BufferPos+=written;

			Buffer[BufferPos]=value ? (byte)1 : (byte)0;
			BufferPos++;
		}

		/// <summary>
		/// Serialize bool value
		/// </summary>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public async ValueTask WriteBoolValueAsync(bool value, CancellationToken cancellationToken = default)
		{
			if(BufferPos>=NestDataStack.CurrentNestData.BufferAvailableLength)
				await FlushAsync(cancellationToken: cancellationToken)
					.ConfigureAwait(false);

			if(BufferPos>=NestDataStack.CurrentNestData.BufferAvailableLength)
				throw new ArgumentOutOfRangeException();

			Buffer[BufferPos]=value ? (byte)1 : (byte)0;
			BufferPos++;
		}

		/// <summary>
		/// Serialize fixed length decimal value
		/// </summary>
		/// <param name="fieldNo">Field unique number</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public async ValueTask WriteDecimalAsync(int fieldNo, decimal value, CancellationToken cancellationToken = default)
		{
			if(NestDataStack.CurrentNestData.BufferAvailableLength-BufferPos<5+1+16)
				await FlushAsync(cancellationToken: cancellationToken)
					.ConfigureAwait(false);

			await EnterObjectAsync(fieldNo: fieldNo, cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			WriteDecimalLittleEndian(destination: new Span<byte>(Buffer, BufferPos, 16), value: value);
			BufferPos+=16;

			await LeaveObjectAsync(cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public void WriteDecimalLittleEndian(Span<byte> destination, decimal value)
		{
			int[] buf;

			buf=decimal.GetBits(value);
			System.Runtime.InteropServices.MemoryMarshal.Cast<int, byte>(buf)
				.CopyTo(destination);
		}

		/// <summary>
		/// Method can be used for writing length delimited data
		/// </summary>
		/// <typeparam name="TState">Type of user state</typeparam>
		/// <param name="state">User state</param>
		/// <param name="writeFunc">Write method</param>
		/// <returns>Number of bytes written</returns>
		public async ValueTask<int> WriteLengthDelimitedChunkAsync<TState>(TState state, Func<TState, Memory<byte>, ValueTask<int>> writeFunc)
		{
			int written;

			written=await writeFunc(state, GetWriteAvailableSpace())
				.ConfigureAwait(false);

			MarkBytesAsWritten(written: written);

			return written;
		}

#if NETCOREAPP
		/// <summary>
		/// Returns buffer for saving data. Returned buffer can be written. After write <see cref="MarkBytesAsWritten(int)"/> method have to be called.
		/// </summary>
		/// <returns>Buffer available for write</returns>
		public Memory<byte> GetWriteAvailableSpace()
		{
			return new Memory<byte>(Buffer, BufferPos, NestDataStack.CurrentNestData.BufferAvailableLength-BufferPos);
		}
#else
		/// <summary>
		/// Returns buffer for saving data. Returned buffer can be written. After write <see cref="MarkBytesAsWritten(int)"/> method have to be called.
		/// </summary>
		/// <returns>Buffer available for write</returns>
		public ArraySegment<byte> GetWriteAvailableSpace()
		{
			return new ArraySegment<byte>(Buffer, BufferPos, NestDataStack.CurrentNestData.BufferAvailableLength-BufferPos);
		}
#endif

		/// <summary>
		/// This method have to be called after saving data to buffer acquired by <see cref="GetWriteAvailableSpace"/> method.
		/// Method marks part of buffer as written.
		/// </summary>
		/// <param name="written">Written bytes count</param>
		public void MarkBytesAsWritten(int written)
		{
			BufferPos+=written;
		}

		public async ValueTask WriteBytesArrayAsync(int fieldNo, ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
		{
			await EnterObjectAsync(fieldNo: fieldNo, cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			await WriteBytesArrayContentAsync(value: value, cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			await LeaveObjectAsync(cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteBytesArrayContentAsync(ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
		{
			ReadOnlyMemory<byte> sourceChunk;
			int byteIndex = 0, actualSpace;

			while(byteIndex<value.Length)
			{
				actualSpace=NestDataStack.CurrentNestData.BufferAvailableLength-BufferPos;

				//Ensure there is a place for at least one byte
				if(actualSpace<1)
				{
					await FlushAsync(cancellationToken: cancellationToken)
						.ConfigureAwait(false);
					actualSpace=NestDataStack.CurrentNestData.BufferAvailableLength-BufferPos;
				}

				sourceChunk=value.Slice(byteIndex, Math.Min(value.Length-byteIndex, actualSpace));

				sourceChunk.CopyTo(new Memory<byte>(Buffer, BufferPos, actualSpace));

				BufferPos+=sourceChunk.Length;
				byteIndex+=sourceChunk.Length;
			}
		}

		public async ValueTask WriteStringAsync(int fieldNo, string value, CancellationToken cancellationToken = default)
		{
			if(value!=null)
			{
#if NETCOREAPP
				await WriteStringAsync(fieldNo: fieldNo, value.AsMemory(), cancellationToken: cancellationToken)
					.ConfigureAwait(false);
#else
				await WriteStringAsync(fieldNo: fieldNo, value: value, start: 0, length: value.Length, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
#endif
			}
		}

		public async ValueTask WriteStringAsync(int fieldNo, string value, int start, int length, CancellationToken cancellationToken = default)
		{
			await EnterObjectAsync(fieldNo: fieldNo, cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			if(0<length)
			{
#if NETCOREAPP
				await WriteStringContentAsync(value: value.AsMemory(start, length), cancellationToken: cancellationToken)
					.ConfigureAwait(false);
#else
			await WriteStringContentAsync(value: value, start: start, length: length, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
#endif
			}

			await LeaveObjectAsync(cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteStringContentAsync(string value, int start, int length, CancellationToken cancellationToken = default)
		{
			int charIndex = 0, charsProcessed, written, actualSpace;
#if NETCOREAPP
			bool flush;
#else
			int maxChars;
#endif
			bool completed;

			StringEncoder.Reset();
			do
			{
				actualSpace=NestDataStack.CurrentNestData.BufferAvailableLength-BufferPos;

				//Ensure there is a place for at least one character
				if(actualSpace<MaxBytesPerChar)
				{
					await FlushAsync(cancellationToken: cancellationToken)
						.ConfigureAwait(false);
					actualSpace=NestDataStack.CurrentNestData.BufferAvailableLength-BufferPos;
				}

#if NETCOREAPP
				flush=StringEncoding.GetMaxByteCount(length-charIndex)<=actualSpace;
				StringEncoder.Convert(value.AsSpan(start+charIndex, length-charIndex), new Span<byte>(Buffer, BufferPos, actualSpace), flush, out charsProcessed, out written, out completed);
#else
				maxChars=actualSpace/MaxBytesPerChar;
				charsProcessed=length-charIndex;
				completed=(charsProcessed<=maxChars);
				if(!completed)
					charsProcessed=maxChars;
				written=StringEncoding.GetBytes(value, start+charIndex, charsProcessed, Buffer, BufferPos);
#endif

				BufferPos+=written;
				charIndex+=charsProcessed;
			}
			while(!completed);
		}

#if NETCOREAPP
		public async ValueTask WriteStringAsync(int fieldNo, ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
		{
			await EnterObjectAsync(fieldNo: fieldNo, cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			if(0<value.Length)
			{
				await WriteStringContentAsync(value: value, cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}

			await LeaveObjectAsync(cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteStringContentAsync(ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
		{
			int charIndex=0, charsProcessed, written, actualSpace;
			bool completed, flush;

			StringEncoder.Reset();
			do
			{
				actualSpace=NestDataStack.CurrentNestData.BufferAvailableLength-BufferPos;

				//Ensure there is a place for at least one character
				if(actualSpace<MaxBytesPerChar)
				{
					await FlushAsync(cancellationToken: cancellationToken)
						.ConfigureAwait(false);
					actualSpace=NestDataStack.CurrentNestData.BufferAvailableLength-BufferPos;
				}

				flush=StringEncoding.GetMaxByteCount(value.Length-charIndex)<=actualSpace;
				StringEncoder.Convert(value.Span.Slice(charIndex), new Span<byte>(Buffer, BufferPos, actualSpace), flush, out charsProcessed, out written, out completed);

				BufferPos+=written;
				charIndex+=charsProcessed;
			}
			while(!completed);
		}
#endif

		/// <summary>
		/// Enter nest object
		/// </summary>
		/// <param name="fieldNo">Field unique number</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public async ValueTask EnterObjectAsync(int fieldNo, CancellationToken cancellationToken = default)
		{
			int headerLength, bufferAvailableLength;

			//Save type and fieldNo
			headerLength=await WriteFieldHeaderAsync(type: Internal.WireType.LengthDelimited, fieldNo: fieldNo, minPlaceAfterHeader: SizeOfLength, cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			//Leave place for length of object (2 bytes)
			BufferPos+=SizeOfLength;
			headerLength+=SizeOfLength;

			bufferAvailableLength=Math.Min(NestDataStack.CurrentNestData.BufferAvailableLength, BufferPos+ushort.MaxValue);

			//Enter new nest level - save index where this nest level starts
			this.NestDataStack.Push(fieldNo: fieldNo, headerLength: headerLength, levelDataStartIndex: BufferPos, bufferAvailableLength: bufferAvailableLength);
		}

		/// <summary>
		/// Leave nest object
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public ValueTask LeaveObjectAsync(CancellationToken cancellationToken = default)
		{
			SaveLeavingObjectData(nestData: NestDataStack.Pop());
			return new ValueTask();
		}

		/// <summary>
		/// Writes header data after leaving nest object
		/// </summary>
		/// <param name="nestData">Leaving nest object data</param>
		protected void SaveLeavingObjectData(NestDataWrite nestData)
		{
			int nestLength;

			//Calculate length of leaving object
			nestLength=BufferPos-nestData.LevelDataStartIndex;

			//Save length
			BinaryPrimitives.WriteUInt16LittleEndian(destination: new Span<byte>(Buffer, nestData.LevelDataStartIndex-SizeOfLength, SizeOfLength), value: (ushort)nestLength);
		}

		/// <summary>
		/// Write current buffer content to stream. Nest context is preserved.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public virtual async ValueTask FlushAsync(CancellationToken cancellationToken = default)
		{
			NestDataWrite nestData, parentNestData;
			int i;

			if(0<BufferPos)
			{
				//Write sizes of all nest levels
				for(i=1; i<=NestDataStack.CurrentNestLevel; i++)//Don't write for level zero
				{
					nestData=NestDataStack[i];
					SaveLeavingObjectData(nestData: nestData);
				}

				//Save Buffer
#if NETCOREAPP
				await Stream.WriteAsync(new Memory<byte>(Buffer, 0, BufferPos), cancellationToken)
					.ConfigureAwait(false);
#else
				await Stream.WriteAsync(Buffer, 0, BufferPos, cancellationToken)
					.ConfigureAwait(false);
#endif

				//Adjust nest levels state
				for(i=1, parentNestData=NestDataStack[0]; i<=NestDataStack.CurrentNestLevel; i++, parentNestData=nestData)//Don't change level zero
				{
					nestData=NestDataStack[i];

					//Move nest field header (wire type with field no) to new position
					Array.Copy(Buffer, nestData.LevelDataStartIndex-nestData.HeaderLength, Buffer, parentNestData.LevelDataStartIndex, nestData.HeaderLength-SizeOfLength);
					//Set new index and length
					nestData.LevelDataStartIndex=parentNestData.LevelDataStartIndex+nestData.HeaderLength;
					nestData.BufferAvailableLength=Math.Min(parentNestData.BufferAvailableLength, nestData.LevelDataStartIndex+ushort.MaxValue);
				}

				//Set new BufferPos
				BufferPos=NestDataStack.CurrentNestData.LevelDataStartIndex;
			}
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
					// TODO: wyczyść stan zarządzany (obiekty zarządzane).
					try
					{
						FlushAsync().AsTask().Wait();
					}
					catch
					{ }

					if(!LeaveOpen)
						this.Stream.Dispose();
				}

				// TODO: Zwolnić niezarządzane zasoby (niezarządzane obiekty) i przesłonić poniższy finalizator.
				// TODO: ustaw wartość null dla dużych pól.
				if(this.Buffer!=null)
					System.Buffers.ArrayPool<byte>.Shared.Return(this.Buffer, this.ClearBufferAfterUse);//Ta funkcja koniecznie musi być wywołana, inaczej będzie następował wyciek pamięci
				this.Buffer=null;

				disposedValue = true;
			}
		}

		// TODO: Przesłonić finalizator tylko w sytuacji, gdy powyższa metoda Dispose(bool disposing) zawiera kod służący do zwalniania niezarządzanych zasobów.
		~ProtoStreamWriter()
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