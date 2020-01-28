using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WojciechMikołajewicz.ProtoStreamReaderWriter.ProtoStreamWriterInternalModel;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter
{
	partial class ProtoStreamWriter
	{
		public async ValueTask WriteByteArrayAsync(int fieldNo, ReadOnlyMemory<byte> byteArray, CancellationToken cancellationToken = default)
		{
			int headerLength, sizeLength, bufferPos, i;
			ulong fieldHeader;
			
			fieldHeader=CalculateFieldHeader(fieldNo: fieldNo, wireType: WireType.LengthDelimited);
			//Try write field header, byte array size and byte array itself
			if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerLength)
				|| !Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos+headerLength), value: fieldHeader, written: out sizeLength)
				|| !byteArray.TryCopyTo(this.Buffer.AsMemory(this.BufferPos+headerLength+sizeLength)))
			{
				//There was insufficient space in the Buffer. Flush and try again
				await FlushAsync(flushStream: false, cancellationToken: cancellationToken)
					.ConfigureAwait(false);

				//Try again write field header, byte array size and byte array itself
				if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerLength)
					|| !Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos+headerLength), value: fieldHeader, written: out sizeLength)
					|| !byteArray.TryCopyTo(this.Buffer.AsMemory(this.BufferPos+headerLength+sizeLength)))
				{
					//Even after flush there was no space to store everything, so use big array strategy
					bufferPos=this.Buffer.Length;

					//Write byta array header from right to left
					WriteFieldHeaderFromRight(fieldHeader);

					//Write nested objects structure from right to left
					for(i=this.NestDatasIndex-1; 0<=i; i--)
						WriteFieldHeaderFromRight(this.NestDatas[i].FieldHeader);

#if NETSTANDARD2_0
					//Write object nesting structure
					await this.Stream.WriteAsync(this.Buffer, bufferPos, this.Buffer.Length-bufferPos, cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					//Write byte array itself
					bufferPos=0;
					while(bufferPos<byteArray.Length)
					{
						//Get size of array chunk
						sizeLength=Math.Min(byteArray.Length-bufferPos, this.Buffer.Length-this.BufferPos);

						byteArray.Slice(bufferPos, sizeLength).CopyTo(this.Buffer.AsMemory(this.BufferPos));

						await this.Stream.WriteAsync(this.Buffer, this.BufferPos, sizeLength, cancellationToken: cancellationToken)
							.ConfigureAwait(false);

						bufferPos+=sizeLength;
					}
#else
					//Write object nesting structure
					await this.Stream.WriteAsync(this.Buffer.AsMemory(bufferPos, this.Buffer.Length-bufferPos), cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					//Write byte array itself
					await this.Stream.WriteAsync(byteArray, cancellationToken: cancellationToken)
						.ConfigureAwait(false);
#endif

					return;
				}
			}

			//If we are here, field header, byte array size and byte array itself was written, actualize BufferPos
			this.BufferPos+=headerLength+sizeLength+byteArray.Length;

			//Method writes from right field header and size
			void WriteFieldHeaderFromRight(ulong in_FieldHeader)
			{
				headerLength=Base128.GetRequiredBytesUInt32((uint)(this.Buffer.Length-bufferPos+byteArray.Length));
				bufferPos-=headerLength;
				if(bufferPos<this.BufferPos || !Base128.TryWriteUInt32(destination: this.Buffer.AsSpan(bufferPos, headerLength), value: (uint)byteArray.Length, minBytesToWrite: headerLength, written: out _))
					throw new InternalBufferOverflowException("Cannot write field header, too many nested objects");

				sizeLength=Base128.GetRequiredBytesUInt64(in_FieldHeader);
				bufferPos-=sizeLength;
				if(bufferPos<this.BufferPos || !Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(bufferPos, sizeLength), value: in_FieldHeader, minBytesToWrite: sizeLength, written: out _))
					throw new InternalBufferOverflowException("Cannot write field header, too many nested objects");
			}
		}

		public async ValueTask WriteByteArrayAsync(int fieldNo, IEnumerable<ReadOnlyMemory<byte>> byteArrays, CancellationToken cancellationToken = default)
		{
			int bufferPos, chunkLength;

			await this.ObjectEnterAsync(fieldNo: fieldNo, cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			foreach(var byteArray in byteArrays)
			{
				bufferPos=0;

				while(bufferPos<byteArray.Length)
				{
					if(this.Buffer.Length<=this.BufferPos)
					{
						await this.FlushAsync(flushStream: false, cancellationToken: cancellationToken)
							.ConfigureAwait(false);

						if(this.Buffer.Length<=this.BufferPos)
							throw new InternalBufferOverflowException("Cannot write field header, too many nested objects");
					}

					chunkLength=Math.Min(byteArray.Length-bufferPos, this.Buffer.Length-this.BufferPos);
					byteArray.Slice(bufferPos, chunkLength).CopyTo(this.Buffer.AsMemory(this.BufferPos));
					bufferPos+=chunkLength;
					this.BufferPos+=chunkLength;
				}
			}

			this.ObjectLeave();
		}
	}
}