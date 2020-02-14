using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WojciechMikołajewicz.ProtoStreamReaderWriter.InternalModel;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter
{
	partial class ProtoStreamReader
	{
		#region Method tables
		private static readonly ReadIntegerAsyncHandler<long>[] ReadLongMethods = new ReadIntegerAsyncHandler<long>[]
		{
			ReadVarInt64Async,//VarInt = 0
			ReadFixedInt64Async,//Fixed64 = 1
			ReadUnsuportedToLongAsync,//LengthDelimited = 2
			ReadUnsuportedToLongAsync,//3
			ReadUnsuportedToLongAsync,//4
			ReadFixed32ToLongAsync,//Fixed32 = 5
			ReadUnsuportedToLongAsync,//6
			ReadUnsuportedToLongAsync,//7
		};
		private static readonly ReadIntegerAsyncHandler<ulong>[] ReadULongMethods = new ReadIntegerAsyncHandler<ulong>[]
		{
			ReadVarUInt64Async,//VarInt = 0
			ReadFixedUInt64Async,//Fixed64 = 1
			ReadUnsuportedToULongAsync,//LengthDelimited = 2
			ReadUnsuportedToULongAsync,//3
			ReadUnsuportedToULongAsync,//4
			ReadFixed32ToULongAsync,//Fixed32 = 5
			ReadUnsuportedToULongAsync,//6
			ReadUnsuportedToULongAsync,//7
		};
		private static readonly ReadIntegerAsyncHandler<int>[] ReadIntMethods = new ReadIntegerAsyncHandler<int>[]
		{
			ReadVarInt32Async,//VarInt = 0
			ReadFixed64ToIntAsync,//Fixed64 = 1
			ReadUnsuportedToIntAsync,//LengthDelimited = 2
			ReadUnsuportedToIntAsync,//3
			ReadUnsuportedToIntAsync,//4
			ReadFixedInt32Async,//Fixed32 = 5
			ReadUnsuportedToIntAsync,//6
			ReadUnsuportedToIntAsync,//7
		};
		private static readonly ReadIntegerAsyncHandler<uint>[] ReadUIntMethods = new ReadIntegerAsyncHandler<uint>[]
		{
			ReadVarUInt32Async,//VarInt = 0
			ReadFixed64ToUIntAsync,//Fixed64 = 1
			ReadUnsuportedToUIntAsync,//LengthDelimited = 2
			ReadUnsuportedToUIntAsync,//3
			ReadUnsuportedToUIntAsync,//4
			ReadFixedUInt32Async,//Fixed32 = 5
			ReadUnsuportedToUIntAsync,//6
			ReadUnsuportedToUIntAsync,//7
		};
		#endregion

		#region ReadVarInt methods
		private static async ValueTask<uint> ReadVarUInt32Async(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			uint value;
			int read;

			if(!Base128.TryReadUInt32(source: psr.Buffer.AsSpan(psr.BufferPos, psr.BufferPopulatedLength-psr.BufferPos), value: out value, read: out read))
			{
				if(!await psr.PopulateVarIntAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream. Cannot read VarInt value.");

				if(!Base128.TryReadUInt32(source: psr.Buffer.AsSpan(psr.BufferPos, psr.BufferPopulatedLength-psr.BufferPos), value: out value, read: out read))
					throw new Exception("Cannot read VarInt value. Protocol error");
			}
			psr.BufferPos+=read;
			return value;
		}

		private static async ValueTask<int> ReadVarInt32Async(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			int value, read;

			if(!Base128.TryReadInt32(source: psr.Buffer.AsSpan(psr.BufferPos, psr.BufferPopulatedLength-psr.BufferPos), value: out value, read: out read))
			{
				if(!await psr.PopulateVarIntAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream. Cannot read VarInt value.");

				if(!Base128.TryReadInt32(source: psr.Buffer.AsSpan(psr.BufferPos, psr.BufferPopulatedLength-psr.BufferPos), value: out value, read: out read))
					throw new Exception("Cannot read VarInt value. Protocol error");
			}
			psr.BufferPos+=read;
			return value;
		}

		private static async ValueTask<ulong> ReadVarUInt64Async(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			ulong value;
			int read;

			if(!Base128.TryReadUInt64(source: psr.Buffer.AsSpan(psr.BufferPos, psr.BufferPopulatedLength-psr.BufferPos), value: out value, read: out read))
			{
				if(!await psr.PopulateVarIntAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream. Cannot read VarInt value.");

				if(!Base128.TryReadUInt64(source: psr.Buffer.AsSpan(psr.BufferPos, psr.BufferPopulatedLength-psr.BufferPos), value: out value, read: out read))
					throw new Exception("Cannot read VarInt value. Protocol error");
			}
			psr.BufferPos+=read;
			return value;
		}

		private static async ValueTask<long> ReadVarInt64Async(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			long value;
			int read;

			if(!Base128.TryReadInt64(source: psr.Buffer.AsSpan(psr.BufferPos, psr.BufferPopulatedLength-psr.BufferPos), value: out value, read: out read))
			{
				if(!await psr.PopulateVarIntAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream. Cannot read VarInt value.");

				if(!Base128.TryReadInt64(source: psr.Buffer.AsSpan(psr.BufferPos, psr.BufferPopulatedLength-psr.BufferPos), value: out value, read: out read))
					throw new Exception("Cannot read VarInt value. Protocol error");
			}
			psr.BufferPos+=read;
			return value;
		}
		#endregion
		#region ReadFixedInt methods
		private static async ValueTask<uint> ReadFixedUInt32Async(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			uint value;

			if(!BinaryPrimitives.TryReadUInt32LittleEndian(source: psr.Buffer.AsSpan(psr.BufferPos, psr.BufferPopulatedLength-psr.BufferPos), value: out value))
			{
				if(!await psr.PopulateFixedAsync(length: sizeof(uint), cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream. Cannot read uint value.");

				if(!BinaryPrimitives.TryReadUInt32LittleEndian(source: psr.Buffer.AsSpan(psr.BufferPos, psr.BufferPopulatedLength-psr.BufferPos), value: out value))
					throw new Exception("Cannot read uint value. Protocol error");
			}
			psr.BufferPos+=sizeof(uint);
			return value;
		}

		private static async ValueTask<int> ReadFixedInt32Async(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			int value;

			if(!BinaryPrimitives.TryReadInt32LittleEndian(source: psr.Buffer.AsSpan(psr.BufferPos, psr.BufferPopulatedLength-psr.BufferPos), value: out value))
			{
				if(!await psr.PopulateFixedAsync(length: sizeof(int), cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream. Cannot read int value.");

				if(!BinaryPrimitives.TryReadInt32LittleEndian(source: psr.Buffer.AsSpan(psr.BufferPos, psr.BufferPopulatedLength-psr.BufferPos), value: out value))
					throw new Exception("Cannot read int value. Protocol error");
			}
			psr.BufferPos+=sizeof(int);
			return value;
		}

		private static async ValueTask<ulong> ReadFixedUInt64Async(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			ulong value;

			if(!BinaryPrimitives.TryReadUInt64LittleEndian(source: psr.Buffer.AsSpan(psr.BufferPos, psr.BufferPopulatedLength-psr.BufferPos), value: out value))
			{
				if(!await psr.PopulateFixedAsync(length: sizeof(ulong), cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream. Cannot read ulong value.");

				if(!BinaryPrimitives.TryReadUInt64LittleEndian(source: psr.Buffer.AsSpan(psr.BufferPos, psr.BufferPopulatedLength-psr.BufferPos), value: out value))
					throw new Exception("Cannot read ulong value. Protocol error");
			}
			psr.BufferPos+=sizeof(ulong);
			return value;
		}

		private static async ValueTask<long> ReadFixedInt64Async(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			long value;

			if(!BinaryPrimitives.TryReadInt64LittleEndian(source: psr.Buffer.AsSpan(psr.BufferPos, psr.BufferPopulatedLength-psr.BufferPos), value: out value))
			{
				if(!await psr.PopulateFixedAsync(length: sizeof(long), cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream. Cannot read long value.");

				if(!BinaryPrimitives.TryReadInt64LittleEndian(source: psr.Buffer.AsSpan(psr.BufferPos, psr.BufferPopulatedLength-psr.BufferPos), value: out value))
					throw new Exception("Cannot read long value. Protocol error");
			}
			psr.BufferPos+=sizeof(long);
			return value;
		}
		#endregion
		#region Converting methods
		private static async ValueTask<int> ReadFixed64ToIntAsync(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			long value;

			value=await ReadFixedInt64Async(psr: psr, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
			return checked((int)value);
		}

		private static ValueTask<int> ReadUnsuportedToIntAsync(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			throw new NotSupportedException("Unsuported wire type while deserializing to int");
		}
		
		private static async ValueTask<uint> ReadFixed64ToUIntAsync(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			ulong value;

			value=await ReadFixedUInt64Async(psr: psr, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
			return checked((uint)value);
		}

		private static ValueTask<uint> ReadUnsuportedToUIntAsync(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			throw new NotSupportedException("Unsuported wire type while deserializing to uint");
		}
		
		private static async ValueTask<long> ReadFixed32ToLongAsync(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			return await ReadFixedInt32Async(psr: psr, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		private static ValueTask<long> ReadUnsuportedToLongAsync(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			throw new NotSupportedException("Unsuported wire type while deserializing to long");
		}
		
		private static async ValueTask<ulong> ReadFixed32ToULongAsync(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			return await ReadFixedUInt32Async(psr: psr, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		private static ValueTask<ulong> ReadUnsuportedToULongAsync(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			throw new NotSupportedException("Unsuported wire type while deserializing to ulong");
		}
		#endregion

		public async ValueTask<int> ReadIntAsync(WireType wireType, CancellationToken cancellationToken = default)
		{
			return await ReadIntMethods[(int)wireType](psr: this, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask<uint> ReadUIntAsync(WireType wireType, CancellationToken cancellationToken = default)
		{
			return await ReadUIntMethods[(int)wireType](psr: this, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask<long> ReadLongAsync(WireType wireType, CancellationToken cancellationToken = default)
		{
			return await ReadLongMethods[(int)wireType](psr: this, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask<ulong> ReadULongAsync(WireType wireType, CancellationToken cancellationToken = default)
		{
			return await ReadULongMethods[(int)wireType](psr: this, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask<bool> ReadBoolAsync(WireType wireType, CancellationToken cancellationToken = default)
		{
			return 0!=await ReadUIntMethods[(int)wireType](psr: this, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask<double> ReadDoubleAsync(WireType wireType, CancellationToken cancellationToken = default)
		{
			return BitConverter.Int64BitsToDouble(await ReadLongMethods[(int)wireType](psr: this, cancellationToken: cancellationToken)
				.ConfigureAwait(false));
		}

		public async ValueTask<float> ReadSingleAsync(WireType wireType, CancellationToken cancellationToken = default)
		{
#if NETSTANDARD2_0
			return Int32BitsToSingle(await ReadIntMethods[(int)wireType](psr: this, cancellationToken: cancellationToken)
				.ConfigureAwait(false));
#else
			return BitConverter.Int32BitsToSingle(await ReadIntMethods[(int)wireType](psr: this, cancellationToken: cancellationToken)
				.ConfigureAwait(false));
#endif
		}

#if NETSTANDARD2_0
		private static float Int32BitsToSingle(int value)
		{
			Span<float> buffer = stackalloc float[1];

			System.Runtime.InteropServices.MemoryMarshal.Cast<float, int>(buffer)[0]=value;

			return buffer[0];
		}
#endif

		public async ValueTask<DateTime> ReadDateTimeAsync(WireType wireType, CancellationToken cancellationToken = default)
		{
			return new DateTime(await ReadLongMethods[(int)wireType](psr: this, cancellationToken: cancellationToken)
				.ConfigureAwait(false));
		}

		public async ValueTask<TimeSpan> ReadTimeSpanAsync(WireType wireType, CancellationToken cancellationToken = default)
		{
			return new TimeSpan(await ReadLongMethods[(int)wireType](psr: this, cancellationToken: cancellationToken)
				.ConfigureAwait(false));
		}
	}
}