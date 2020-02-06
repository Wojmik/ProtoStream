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
			await WriteFixedIntFieldAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken, writeFixedIntMethod: BinaryPrimitives.TryWriteInt64LittleEndian, wireType: WireType.Fixed64)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteVarInt64FieldAsync(int fieldNo, long value, CancellationToken cancellationToken = default)
		{
			await WriteVarIntFieldAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken, writeVarIntMethod: Base128.TryWriteInt64)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteFixedUInt64FieldAsync(int fieldNo, ulong value, CancellationToken cancellationToken = default)
		{
			await WriteFixedIntFieldAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken, writeFixedIntMethod: BinaryPrimitives.TryWriteUInt64LittleEndian, wireType: WireType.Fixed64)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteVarUInt64FieldAsync(int fieldNo, ulong value, CancellationToken cancellationToken = default)
		{
			await WriteVarIntFieldAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken, writeVarIntMethod: Base128.TryWriteUInt64)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteFixedInt32FieldAsync(int fieldNo, int value, CancellationToken cancellationToken = default)
		{
			await WriteFixedIntFieldAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken, writeFixedIntMethod: BinaryPrimitives.TryWriteInt32LittleEndian, wireType: WireType.Fixed32)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteVarInt32FieldAsync(int fieldNo, int value, CancellationToken cancellationToken = default)
		{
			await WriteVarIntFieldAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken, writeVarIntMethod: Base128.TryWriteInt32)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteFixedUInt32FieldAsync(int fieldNo, uint value, CancellationToken cancellationToken = default)
		{
			await WriteFixedIntFieldAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken, writeFixedIntMethod: BinaryPrimitives.TryWriteUInt32LittleEndian, wireType: WireType.Fixed32)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteVarUInt32FieldAsync(int fieldNo, uint value, CancellationToken cancellationToken = default)
		{
			await WriteVarIntFieldAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken, writeVarIntMethod: Base128.TryWriteUInt32)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteFloatAsync(int fieldNo, float value, CancellationToken cancellationToken = default)
		{
			int intVal;
#if NETSTANDARD2_0
			intVal=SingleToInt32Bits(value);
#else
			intVal=BitConverter.SingleToInt32Bits(value);
#endif
			
			await WriteFixedIntFieldAsync(fieldNo: fieldNo, value: intVal, cancellationToken: cancellationToken, writeFixedIntMethod: BinaryPrimitives.TryWriteInt32LittleEndian, wireType: WireType.Fixed32)
				.ConfigureAwait(false);
		}

#if NETSTANDARD2_0
		private static int SingleToInt32Bits(float value)
		{
			Span<int> buffer = stackalloc int[1];
			
			System.Runtime.InteropServices.MemoryMarshal.Cast<int, float>(buffer)[0]=value;

			return buffer[0];
		}
#endif

		public async ValueTask WriteDoubleAsync(int fieldNo, double value, CancellationToken cancellationToken = default)
		{
			await WriteFixedIntFieldAsync(fieldNo: fieldNo, value: BitConverter.DoubleToInt64Bits(value), cancellationToken: cancellationToken, writeFixedIntMethod: BinaryPrimitives.TryWriteInt64LittleEndian, wireType: WireType.Fixed64)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteDateTimeFieldAsync(int fieldNo, DateTime value, CancellationToken cancellationToken = default)
		{
			await WriteFixedIntFieldAsync(fieldNo: fieldNo, value: value.Ticks, cancellationToken: cancellationToken, writeFixedIntMethod: BinaryPrimitives.TryWriteInt64LittleEndian, wireType: WireType.Fixed64)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteTimeSpanFieldAsync(int fieldNo, TimeSpan value, CancellationToken cancellationToken = default)
		{
			await WriteVarIntFieldAsync(fieldNo: fieldNo, value: value.Ticks, cancellationToken: cancellationToken, writeVarIntMethod: Base128.TryWriteInt64)
				.ConfigureAwait(false);
		}
		

		private async ValueTask WriteFixedIntFieldAsync<TValue>(int fieldNo, TValue value, CancellationToken cancellationToken, WriteFixedIntHandler<TValue> writeFixedIntMethod, WireType wireType)
			where TValue : struct
		{
			int headerLength;
			ulong fieldHeader;

			fieldHeader=CalculateFieldHeader(fieldNo: fieldNo, wireType: wireType);
			
			//Try write field header and value
			if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerLength)
				|| !writeFixedIntMethod(destination: this.Buffer.AsSpan(this.BufferPos+headerLength), value: value))
			{
				//There was insufficient space in the Buffer. Flush and try again
				await FlushAsync(flushStream: false, cancellationToken: cancellationToken)
					.ConfigureAwait(false);

				//Try again write field header and value
				if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerLength)
					|| !writeFixedIntMethod(destination: this.Buffer.AsSpan(this.BufferPos+headerLength), value: value))
					throw new InternalBufferOverflowException("Cannot write field, too many nested objects");
			}

			this.BufferPos+=headerLength+9-(int)wireType;
		}

		private async ValueTask WriteVarIntFieldAsync<TValue>(int fieldNo, TValue value, CancellationToken cancellationToken, WriteVarIntHandler<TValue> writeVarIntMethod)
			where TValue : struct
		{
			int headerLength, valueLength;
			ulong fieldHeader;

			fieldHeader=CalculateFieldHeader(fieldNo: fieldNo, wireType: WireType.VarInt);
			//Try write field header and value
			if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerLength)
				|| !writeVarIntMethod(destination: this.Buffer.AsSpan(this.BufferPos+headerLength), value: value, written: out valueLength))
			{
				//There was insufficient space in the Buffer. Flush and try again
				await FlushAsync(flushStream: false, cancellationToken: cancellationToken)
					.ConfigureAwait(false);

				//Try again write field header and value
				if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerLength)
					|| !writeVarIntMethod(destination: this.Buffer.AsSpan(this.BufferPos+headerLength), value: value, written: out valueLength))
					throw new InternalBufferOverflowException("Cannot write field, too many nested objects");
			}

			this.BufferPos+=headerLength+valueLength;
		}
	}
}