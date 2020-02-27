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
	partial class ProtoStreamWriter
	{
		public async ValueTask WriteFixedInt64FieldAsync(int fieldNo, long value, CancellationToken cancellationToken = default)
		{
			await WriteFieldAsync(fieldNo: fieldNo, wireType: WireType.Fixed64, value: value, writeMethod: TryWriteFixedInt64, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteVarInt64FieldAsync(int fieldNo, long value, CancellationToken cancellationToken = default)
		{
			await WriteFieldAsync(fieldNo: fieldNo, wireType: WireType.VarInt, value: value, writeMethod: Base128.TryWriteInt64, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteFixedUInt64FieldAsync(int fieldNo, ulong value, CancellationToken cancellationToken = default)
		{
			await WriteFieldAsync(fieldNo: fieldNo, wireType: WireType.Fixed64, value: value, writeMethod: TryWriteFixedUInt64, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteVarUInt64FieldAsync(int fieldNo, ulong value, CancellationToken cancellationToken = default)
		{
			await WriteFieldAsync(fieldNo: fieldNo, wireType: WireType.VarInt, value: value, writeMethod: Base128.TryWriteUInt64, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteFixedInt32FieldAsync(int fieldNo, int value, CancellationToken cancellationToken = default)
		{
			await WriteFieldAsync(fieldNo: fieldNo, wireType: WireType.Fixed32, value: value, writeMethod: TryWriteFixedInt32, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteVarInt32FieldAsync(int fieldNo, int value, CancellationToken cancellationToken = default)
		{
			await WriteFieldAsync(fieldNo: fieldNo, wireType: WireType.VarInt, value: value, writeMethod: Base128.TryWriteInt32, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteFixedUInt32FieldAsync(int fieldNo, uint value, CancellationToken cancellationToken = default)
		{
			await WriteFieldAsync(fieldNo: fieldNo, wireType: WireType.Fixed32, value: value, writeMethod: TryWriteFixedUInt32, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteVarUInt32FieldAsync(int fieldNo, uint value, CancellationToken cancellationToken = default)
		{
			await WriteFieldAsync(fieldNo: fieldNo, wireType: WireType.VarInt, value: value, writeMethod: Base128.TryWriteUInt32, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteBoolAsync(int fieldNo, bool value, CancellationToken cancellationToken = default)
		{
			await WriteFieldAsync(fieldNo: fieldNo, wireType: WireType.VarInt, value: value, writeMethod: TryWriteBool, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteDoubleAsync(int fieldNo, double value, CancellationToken cancellationToken = default)
		{
			await WriteFieldAsync(fieldNo: fieldNo, wireType: WireType.Fixed64, value: value, writeMethod: TryWriteDouble, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteSingleAsync(int fieldNo, float value, CancellationToken cancellationToken = default)
		{
			await WriteFieldAsync(fieldNo: fieldNo, wireType: WireType.Fixed32, value: value, writeMethod: TryWriteSingle, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteDateTimeFieldAsync(int fieldNo, DateTime value, CancellationToken cancellationToken = default)
		{
			await WriteFieldAsync(fieldNo: fieldNo, wireType: WireType.Fixed64, value: value, writeMethod: TryWriteDateTime, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteTimeSpanFieldAsync(int fieldNo, TimeSpan value, CancellationToken cancellationToken = default)
		{
			await WriteFieldAsync(fieldNo: fieldNo, wireType: WireType.VarInt, value: value, writeMethod: TryWriteTimeSpan, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		private async ValueTask WriteFieldAsync<TValue>(int fieldNo, WireType wireType, TValue value, TryWriteSimpleFieldHandler<TValue> writeMethod, CancellationToken cancellationToken)
			where TValue : struct
		{
			int headerLength, valueLength;
			ulong fieldHeader;

			fieldHeader=CalculateFieldHeader(fieldNo: fieldNo, wireType: wireType);
			//Try write field header and value
			if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerLength)
				|| !writeMethod(destination: this.Buffer.AsSpan(this.BufferPos+headerLength), value: value, written: out valueLength))
			{
				//There was insufficient space in the Buffer. Flush and try again
				await FlushAsync(flushStream: false, cancellationToken: cancellationToken)
					.ConfigureAwait(false);

				//Try again write field header and value
				if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerLength)
					|| !writeMethod(destination: this.Buffer.AsSpan(this.BufferPos+headerLength), value: value, written: out valueLength))
					throw new InternalBufferOverflowException("Cannot write field, too many nested objects");
			}

			this.BufferPos+=headerLength+valueLength;
		}

		#region Helper methods
		private static bool TryWriteFixedInt64(Span<byte> destination, long value, out int written)
		{
			written=sizeof(long);
			return BinaryPrimitives.TryWriteInt64LittleEndian(destination: destination, value: value);
		}

		private static bool TryWriteFixedUInt64(Span<byte> destination, ulong value, out int written)
		{
			written=sizeof(ulong);
			return BinaryPrimitives.TryWriteUInt64LittleEndian(destination: destination, value: value);
		}

		private static bool TryWriteFixedInt32(Span<byte> destination, int value, out int written)
		{
			written=sizeof(int);
			return BinaryPrimitives.TryWriteInt32LittleEndian(destination: destination, value: value);
		}

		private static bool TryWriteFixedUInt32(Span<byte> destination, uint value, out int written)
		{
			written=sizeof(uint);
			return BinaryPrimitives.TryWriteUInt32LittleEndian(destination: destination, value: value);
		}

		private static bool TryWriteBool(Span<byte> destination, bool value, out int written)
		{
			return Base128.TryWriteUInt32(destination: destination, value: value ? 1U : 0U, written: out written);
		}

		private static bool TryWriteDouble(Span<byte> destination, double value, out int written)
		{
			written=sizeof(long);
			return BinaryPrimitives.TryWriteInt64LittleEndian(destination: destination, value: BitConverter.DoubleToInt64Bits(value));
		}

		private static bool TryWriteSingle(Span<byte> destination, float value, out int written)
		{
			written=sizeof(int);

#if NETSTANDARD2_0
			Span<int> buffer = stackalloc int[1];
			System.Runtime.InteropServices.MemoryMarshal.Cast<int, float>(buffer)[0]=value;
			return BinaryPrimitives.TryWriteInt32LittleEndian(destination: destination, value: buffer[0]);
#else
			return BinaryPrimitives.TryWriteInt32LittleEndian(destination: destination, value: BitConverter.SingleToInt32Bits(value));
#endif
		}

		private static bool TryWriteDateTime(Span<byte> destination, DateTime value, out int written)
		{
			written=sizeof(long);
			return BinaryPrimitives.TryWriteInt64LittleEndian(destination: destination, value: value.ToBinary());
		}

		private static bool TryWriteTimeSpan(Span<byte> destination, TimeSpan value, out int written)
		{
			return Base128.TryWriteInt64(destination: destination, value: value.Ticks, written: out written);
		}
		#endregion
	}
}