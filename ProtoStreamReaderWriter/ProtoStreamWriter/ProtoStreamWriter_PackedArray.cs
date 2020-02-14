using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter
{
	partial class ProtoStreamWriter
	{
		public async ValueTask WriteArrayFixedInt64Async(int fieldNo, IEnumerable<long> value, CancellationToken cancellationToken = default)
		{
			await WritePackedArrayAsync(fieldNo: fieldNo, value: value, itemMaxSize: sizeof(long), tryWriteMethod: TryWriteFixedInt64, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteArrayVarInt64Async(int fieldNo, IEnumerable<long> value, CancellationToken cancellationToken = default)
		{
			await WritePackedArrayAsync(fieldNo: fieldNo, value: value, itemMaxSize: 10, tryWriteMethod: Base128.TryWriteInt64, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteArrayFixedUInt64Async(int fieldNo, IEnumerable<ulong> value, CancellationToken cancellationToken = default)
		{
			await WritePackedArrayAsync(fieldNo: fieldNo, value: value, itemMaxSize: sizeof(long), tryWriteMethod: TryWriteFixedUInt64, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteArrayVarUInt64Async(int fieldNo, IEnumerable<ulong> value, CancellationToken cancellationToken = default)
		{
			await WritePackedArrayAsync(fieldNo: fieldNo, value: value, itemMaxSize: 10, tryWriteMethod: Base128.TryWriteUInt64, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteArrayFixedInt32Async(int fieldNo, IEnumerable<int> value, CancellationToken cancellationToken = default)
		{
			await WritePackedArrayAsync(fieldNo: fieldNo, value: value, itemMaxSize: sizeof(int), tryWriteMethod: TryWriteFixedInt32, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteArrayVarInt32Async(int fieldNo, IEnumerable<int> value, CancellationToken cancellationToken = default)
		{
			await WritePackedArrayAsync(fieldNo: fieldNo, value: value, itemMaxSize: 5, tryWriteMethod: Base128.TryWriteInt32, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteArrayFixedUInt32Async(int fieldNo, IEnumerable<uint> value, CancellationToken cancellationToken = default)
		{
			await WritePackedArrayAsync(fieldNo: fieldNo, value: value, itemMaxSize: sizeof(uint), tryWriteMethod: TryWriteFixedUInt32, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteArrayVarUInt32Async(int fieldNo, IEnumerable<uint> value, CancellationToken cancellationToken = default)
		{
			await WritePackedArrayAsync(fieldNo: fieldNo, value: value, itemMaxSize: 5, tryWriteMethod: Base128.TryWriteUInt32, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteArrayBoolAsync(int fieldNo, IEnumerable<bool> value, CancellationToken cancellationToken = default)
		{
			await WritePackedArrayAsync(fieldNo: fieldNo, value: value, itemMaxSize: 1, tryWriteMethod: TryWriteBool, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteArrayDoubleAsync(int fieldNo, IEnumerable<double> value, CancellationToken cancellationToken = default)
		{
			await WritePackedArrayAsync(fieldNo: fieldNo, value: value, itemMaxSize: sizeof(long), tryWriteMethod: TryWriteDouble, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteArraySingleAsync(int fieldNo, IEnumerable<float> value, CancellationToken cancellationToken = default)
		{
			await WritePackedArrayAsync(fieldNo: fieldNo, value: value, itemMaxSize: sizeof(int), tryWriteMethod: TryWriteSingle, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteArrayDateTimeAsync(int fieldNo, IEnumerable<DateTime> value, CancellationToken cancellationToken = default)
		{
			await WritePackedArrayAsync(fieldNo: fieldNo, value: value, itemMaxSize: sizeof(long), tryWriteMethod: TryWriteDateTime, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteArrayTimeSpanAsync(int fieldNo, IEnumerable<TimeSpan> value, CancellationToken cancellationToken = default)
		{
			await WritePackedArrayAsync(fieldNo: fieldNo, value: value, itemMaxSize: 10, tryWriteMethod: TryWriteTimeSpan, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		protected async ValueTask WritePackedArrayAsync<T>(int fieldNo, IEnumerable<T> value, int itemMaxSize, InternalModel.TryWriteSimpleFieldHandler<T> tryWriteMethod, CancellationToken cancellationToken)
			where T : struct
		{
			int headerSize, lengthHoleSize, bytesUsed, count = 0;
			ulong fieldHeader;
			bool knownSize, dataAvailable;

			if(value!=null)
			{
				fieldHeader=CalculateFieldHeader(fieldNo: fieldNo, wireType: WireType.LengthDelimited);

				if(knownSize=value is IReadOnlyCollection<int>)
					count=(value as IReadOnlyCollection<int>).Count;

				using(var enumerator = value.GetEnumerator())
				{
					dataAvailable=enumerator.MoveNext();

					do
					{
						//Try write field header and reserve space for field size and one item (to prevent endles loop)
						if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerSize)
							|| this.Buffer.Length-this.BufferPos-headerSize<(lengthHoleSize=Base128.GetRequiredBytesUInt32(knownSize ? (uint)Math.Min((long)count*itemMaxSize, this.Buffer.Length-this.BufferPos-headerSize) : (uint)(this.Buffer.Length-this.BufferPos-headerSize)))+itemMaxSize)
						{
							//There was insufficient space in the Buffer. Flush and try again
							await FlushAsync(flushStream: false, cancellationToken: cancellationToken)
								.ConfigureAwait(false);

							//Try again write field header and reserve space for field size and one item (to prevent endles loop)
							if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerSize)
								|| this.Buffer.Length-this.BufferPos-headerSize<(lengthHoleSize=Base128.GetRequiredBytesUInt32(knownSize ? (uint)Math.Min((long)count*itemMaxSize, this.Buffer.Length-this.BufferPos-headerSize) : (uint)(this.Buffer.Length-this.BufferPos-headerSize)))+itemMaxSize)
								throw new InternalBufferOverflowException("Cannot write field, too many nested objects");
						}

						//If we are here, field header was written and space for field size reserved, actualize BufferPos
						this.BufferPos+=headerSize+lengthHoleSize;

						bytesUsed=0;
						while(dataAvailable)
						{
							if(!tryWriteMethod(destination: this.Buffer.AsSpan(this.BufferPos+bytesUsed), value: enumerator.Current, out headerSize))
								break;
							bytesUsed+=headerSize;
							count--;

							dataAvailable=enumerator.MoveNext();
						}

						//Save length in bytes of written array items
						Base128.WriteUInt32(destination: this.Buffer.AsSpan(this.BufferPos-lengthHoleSize, lengthHoleSize), value: (uint)bytesUsed, minBytesToWrite: lengthHoleSize, written: out _);
						this.BufferPos+=bytesUsed;
					}
					while(dataAvailable);
				}
			}
		}
	}
}