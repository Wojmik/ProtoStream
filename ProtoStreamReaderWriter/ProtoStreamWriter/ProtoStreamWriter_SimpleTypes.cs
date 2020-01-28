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
			await WriteFixedIntFieldAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken, writeFixedIntMethod: BinaryPrimitives.TryWriteInt64LittleEndian, valueSize: sizeof(long))
				.ConfigureAwait(false);
		}

		public async ValueTask WriteVarInt64FieldAsync(int fieldNo, long value, CancellationToken cancellationToken = default)
		{
			await WriteVarIntFieldAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken, writeVarIntMethod: Base128.TryWriteInt64)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteFixedUInt64FieldAsync(int fieldNo, ulong value, CancellationToken cancellationToken = default)
		{
			await WriteFixedIntFieldAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken, writeFixedIntMethod: BinaryPrimitives.TryWriteUInt64LittleEndian, valueSize: sizeof(ulong))
				.ConfigureAwait(false);
		}

		public async ValueTask WriteVarUInt64FieldAsync(int fieldNo, ulong value, CancellationToken cancellationToken = default)
		{
			await WriteVarIntFieldAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken, writeVarIntMethod: Base128.TryWriteUInt64)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteFixedInt32FieldAsync(int fieldNo, int value, CancellationToken cancellationToken = default)
		{
			await WriteFixedIntFieldAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken, writeFixedIntMethod: BinaryPrimitives.TryWriteInt32LittleEndian, valueSize: sizeof(int))
				.ConfigureAwait(false);
		}

		public async ValueTask WriteVarInt32FieldAsync(int fieldNo, int value, CancellationToken cancellationToken = default)
		{
			await WriteVarIntFieldAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken, writeVarIntMethod: Base128.TryWriteInt32)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteFixedUInt32FieldAsync(int fieldNo, uint value, CancellationToken cancellationToken = default)
		{
			await WriteFixedIntFieldAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken, writeFixedIntMethod: BinaryPrimitives.TryWriteUInt32LittleEndian, valueSize: sizeof(uint))
				.ConfigureAwait(false);
		}

		public async ValueTask WriteVarUInt32FieldAsync(int fieldNo, uint value, CancellationToken cancellationToken = default)
		{
			await WriteVarIntFieldAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken, writeVarIntMethod: Base128.TryWriteUInt32)
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteDateTimeFieldAsync(int fieldNo, DateTime value, CancellationToken cancellationToken = default)
		{
			await WriteFixedIntFieldAsync(fieldNo: fieldNo, value: value.Ticks, cancellationToken: cancellationToken, writeFixedIntMethod: BinaryPrimitives.TryWriteInt64LittleEndian, valueSize: sizeof(long))
				.ConfigureAwait(false);
		}
		
		public async ValueTask WriteTimeSpanFieldAsync(int fieldNo, TimeSpan value, CancellationToken cancellationToken = default)
		{
			await WriteVarIntFieldAsync(fieldNo: fieldNo, value: value.Ticks, cancellationToken: cancellationToken, writeVarIntMethod: Base128.TryWriteInt64)
				.ConfigureAwait(false);
		}
		

		private async ValueTask WriteFixedIntFieldAsync<TValue>(int fieldNo, TValue value, CancellationToken cancellationToken, WriteFixedIntHandler<TValue> writeFixedIntMethod, int valueSize)
		{
			int headerLength;
			ulong fieldHeader;

			fieldHeader=CalculateFieldHeader(fieldNo: fieldNo, wireType: WireType.Fixed64);
			
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

			this.BufferPos+=headerLength+valueSize;
		}

		private async ValueTask WriteVarIntFieldAsync<TValue>(int fieldNo, TValue value, CancellationToken cancellationToken, WriteVarIntHandler<TValue> writeVarIntMethod)
		{
			int headerLength, valueLength;
			ulong fieldHeader;

			fieldHeader=CalculateFieldHeader(fieldNo: fieldNo, wireType: WireType.Fixed64);
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