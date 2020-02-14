using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter
{
	partial class ProtoStreamReader
	{
		public async ValueTask<List<long>> ReadListFixedInt64Async(List<long> previousValue, CancellationToken cancellationToken = default)
		{
			return await ReadPackedListAsync(previousValue: previousValue, tryReadMethod: TryReadFixedInt64, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask<List<long>> ReadListVarInt64Async(List<long> previousValue, CancellationToken cancellationToken = default)
		{
			return await ReadPackedListAsync(previousValue: previousValue, tryReadMethod: Base128.TryReadInt64, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask<List<ulong>> ReadListFixedUInt64Async(List<ulong> previousValue, CancellationToken cancellationToken = default)
		{
			return await ReadPackedListAsync(previousValue: previousValue, tryReadMethod: TryReadFixedUInt64, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask<List<ulong>> ReadListVarUInt64Async(List<ulong> previousValue, CancellationToken cancellationToken = default)
		{
			return await ReadPackedListAsync(previousValue: previousValue, tryReadMethod: Base128.TryReadUInt64, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask<List<int>> ReadListFixedInt32Async(List<int> previousValue, CancellationToken cancellationToken = default)
		{
			return await ReadPackedListAsync(previousValue: previousValue, tryReadMethod: TryReadFixedInt32, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask<List<int>> ReadListVarInt32Async(List<int> previousValue, CancellationToken cancellationToken = default)
		{
			return await ReadPackedListAsync(previousValue: previousValue, tryReadMethod: Base128.TryReadInt32, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask<List<uint>> ReadListFixedUInt32Async(List<uint> previousValue, CancellationToken cancellationToken = default)
		{
			return await ReadPackedListAsync(previousValue: previousValue, tryReadMethod: TryReadFixedUInt32, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask<List<uint>> ReadListVarUInt32Async(List<uint> previousValue, CancellationToken cancellationToken = default)
		{
			return await ReadPackedListAsync(previousValue: previousValue, tryReadMethod: Base128.TryReadUInt32, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask<List<bool>> ReadListBoolAsync(List<bool> previousValue, CancellationToken cancellationToken = default)
		{
			return await ReadPackedListAsync(previousValue: previousValue, tryReadMethod: TryReadBool, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask<List<double>> ReadListDoubleAsync(List<double> previousValue, CancellationToken cancellationToken = default)
		{
			return await ReadPackedListAsync(previousValue: previousValue, tryReadMethod: TryReadDouble, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask<List<float>> ReadListSingleAsync(List<float> previousValue, CancellationToken cancellationToken = default)
		{
			return await ReadPackedListAsync(previousValue: previousValue, tryReadMethod: TryReadSingle, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask<List<DateTime>> ReadListDateTimeAsync(List<DateTime> previousValue, CancellationToken cancellationToken = default)
		{
			return await ReadPackedListAsync(previousValue: previousValue, tryReadMethod: TryReadDateTime, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask<List<TimeSpan>> ReadListTimeSpanAsync(List<TimeSpan> previousValue, CancellationToken cancellationToken = default)
		{
			return await ReadPackedListAsync(previousValue: previousValue, tryReadMethod: TryReadTimeSpan, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		protected async ValueTask<List<T>> ReadPackedListAsync<T>(List<T> previousValue, InternalModel.TryReadSimpleFieldHandler<T> tryReadMethod, CancellationToken cancellationToken)
			where T : struct
		{
			ulong endObjectPosition;
			int toRead, read;
			T value;

			if(this.NestDatasIndex<0)
				throw new Exception("Not in variable length field. Cannot read list. Protocol error");

			if(previousValue==null)
				previousValue=new List<T>();

			endObjectPosition=this.NestDatas[this.NestDatasIndex].EndObjectPosition;

			toRead=(int)(endObjectPosition-this.ShrinkedBufferLength)-this.BufferPos;

			while(0<toRead)
			{
				if(!tryReadMethod(this.Buffer.AsSpan(this.BufferPos, this.BufferPopulatedLength-this.BufferPos), value: out value, read: out read))
				{
					if(!await this.PopulateFixedAsync(length: toRead, cancellationToken: cancellationToken).ConfigureAwait(false))
						throw new EndOfStreamException("Unexpected end of stream. Cannot read whole list.");
					continue;
				}
				previousValue.Add(value);

				this.BufferPos+=read;
				toRead-=read;
			}

			this.NestDatasIndex--;

			return previousValue;
		}

		#region HelperMethods
		private static bool TryReadFixedInt64(ReadOnlySpan<byte> source, out long value, out int read)
		{
			read=sizeof(long);
			return BinaryPrimitives.TryReadInt64LittleEndian(source: source, value: out value);
		}

		private static bool TryReadFixedUInt64(ReadOnlySpan<byte> source, out ulong value, out int read)
		{
			read=sizeof(ulong);
			return BinaryPrimitives.TryReadUInt64LittleEndian(source: source, value: out value);
		}

		private static bool TryReadFixedInt32(ReadOnlySpan<byte> source, out int value, out int read)
		{
			read=sizeof(int);
			return BinaryPrimitives.TryReadInt32LittleEndian(source: source, value: out value);
		}

		private static bool TryReadFixedUInt32(ReadOnlySpan<byte> source, out uint value, out int read)
		{
			read=sizeof(uint);
			return BinaryPrimitives.TryReadUInt32LittleEndian(source: source, value: out value);
		}
		
		private static bool TryReadBool(ReadOnlySpan<byte> source, out bool value, out int read)
		{
			uint val;
			bool ok;

			ok=Base128.TryReadUInt32(source: source, value: out val, read: out read);
			value=val!=0;
			return ok;
		}

		private static bool TryReadDouble(ReadOnlySpan<byte> source, out double value, out int read)
		{
			long val;
			bool ok;

			read=sizeof(long);
			ok=BinaryPrimitives.TryReadInt64LittleEndian(source: source, value: out val);
			value=BitConverter.Int64BitsToDouble(val);
			return ok;
		}

		private static bool TryReadSingle(ReadOnlySpan<byte> source, out float value, out int read)
		{
			int val;
			bool ok;

			read=sizeof(int);
			ok=BinaryPrimitives.TryReadInt32LittleEndian(source: source, value: out val);
#if NETSTANDARD2_0
			value=Int32BitsToSingle(val);
#else
			value=BitConverter.Int32BitsToSingle(val);
#endif
			return ok;
		}

		private static bool TryReadDateTime(ReadOnlySpan<byte> source, out DateTime value, out int read)
		{
			long val;
			bool ok;

			read=sizeof(long);
			ok=BinaryPrimitives.TryReadInt64LittleEndian(source: source, value: out val);
			value=new DateTime(val);
			return ok;
		}

		private static bool TryReadTimeSpan(ReadOnlySpan<byte> source, out TimeSpan value, out int read)
		{
			long val;
			bool ok;

			ok=Base128.TryReadInt64(source: source, value: out val, read: out read);
			value=new TimeSpan(val);
			return ok;
		}
		#endregion
	}
}