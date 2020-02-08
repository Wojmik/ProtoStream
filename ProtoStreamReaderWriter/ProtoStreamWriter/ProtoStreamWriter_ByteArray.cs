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
			int headerSize, lengthHoleSize, bufferPos, i;
			ulong fieldHeader;
			
			fieldHeader=CalculateFieldHeader(fieldNo: fieldNo, wireType: WireType.LengthDelimited);
			//Try write field header, byte array size and byte array itself
			if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerSize)
				|| !Base128.TryWriteUInt32(destination: this.Buffer.AsSpan(this.BufferPos+headerSize), value: (uint)byteArray.Length, written: out lengthHoleSize)
				|| !byteArray.TryCopyTo(this.Buffer.AsMemory(this.BufferPos+headerSize+lengthHoleSize)))
			{
				//There was insufficient space in the Buffer. Flush and try again
				await FlushAsync(flushStream: false, cancellationToken: cancellationToken)
					.ConfigureAwait(false);

				//Try again write field header, byte array size and byte array itself
				if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerSize)
					|| !Base128.TryWriteUInt32(destination: this.Buffer.AsSpan(this.BufferPos+headerSize), value: (uint)byteArray.Length, written: out lengthHoleSize)
					|| !byteArray.TryCopyTo(this.Buffer.AsMemory(this.BufferPos+headerSize+lengthHoleSize)))
				{
					//Even after flush there was no space to store everything, so use big array strategy
					bufferPos=this.Buffer.Length;

					//Write byte array header from right to left
					WriteFieldHeaderFromRight(fieldHeader, fieldLength: byteArray.Length, bufferPos: ref bufferPos);

					//Write nested objects structure from right to left
					for(i=this.NestDatasIndex-1; 0<=i; i--)
						WriteFieldHeaderFromRight(this.NestDatas[i].FieldHeader, fieldLength: byteArray.Length, bufferPos: ref bufferPos);

#if NETSTANDARD2_0
					//Write object nesting structure
					await this.Stream.WriteAsync(this.Buffer, bufferPos, this.Buffer.Length-bufferPos, cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					//Write byte array itself
					bufferPos=0;
					while(bufferPos<byteArray.Length)
					{
						//Get size of array chunk
						lengthHoleSize=Math.Min(byteArray.Length-bufferPos, this.Buffer.Length-this.BufferPos);

						byteArray.Slice(bufferPos, lengthHoleSize).CopyTo(this.Buffer.AsMemory(this.BufferPos));

						await this.Stream.WriteAsync(this.Buffer, this.BufferPos, lengthHoleSize, cancellationToken: cancellationToken)
							.ConfigureAwait(false);

						bufferPos+=lengthHoleSize;
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
			this.BufferPos+=headerSize+lengthHoleSize+byteArray.Length;
		}

		public async ValueTask WriteByteArrayFromChunksAsync(int fieldNo, ReadOnlyMemory<ReadOnlyMemory<byte>> byteArrayChunks, CancellationToken cancellationToken = default)
		{
			int headerLength, sizeLength, bufferPos, i, dataLength=0;
			ulong fieldHeader;

			//Calculate total data size
			for(i=0; i<byteArrayChunks.Length; i++)
				dataLength+=byteArrayChunks.Span[i].Length;

			fieldHeader=CalculateFieldHeader(fieldNo: fieldNo, wireType: WireType.LengthDelimited);
			//Try write field header, byte array size and check is there a space in the Buffer to write byte array itself
			if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerLength)
				|| !Base128.TryWriteUInt32(destination: this.Buffer.AsSpan(this.BufferPos+headerLength), value: (uint)dataLength, written: out sizeLength)
				|| this.Buffer.Length<this.BufferPos+headerLength+sizeLength+dataLength)
			{
				//There was insufficient space in the Buffer. Flush and try again
				await FlushAsync(flushStream: false, cancellationToken: cancellationToken)
					.ConfigureAwait(false);

				//Try again write field header, byte array size and check is there a space in the Buffer to write byte array itself
				if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerLength)
					|| !Base128.TryWriteUInt32(destination: this.Buffer.AsSpan(this.BufferPos+headerLength), value: (uint)dataLength, written: out sizeLength)
					|| this.Buffer.Length<this.BufferPos+headerLength+sizeLength+dataLength)
				{
					//Even after flush there was no space to store everything, so use big array strategy
					bufferPos=this.Buffer.Length;

					//Write byte array header from right to left
					WriteFieldHeaderFromRight(fieldHeader, fieldLength: dataLength, bufferPos: ref bufferPos);

					//Write nested objects structure from right to left
					for(i=this.NestDatasIndex-1; 0<=i; i--)
						WriteFieldHeaderFromRight(this.NestDatas[i].FieldHeader, fieldLength: dataLength, bufferPos: ref bufferPos);

#if NETSTANDARD2_0
					//Write object nesting structure
					await this.Stream.WriteAsync(this.Buffer, bufferPos, this.Buffer.Length-bufferPos, cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					bufferPos=this.BufferPos;
					//Write all array chunks
					for(i=0; i<byteArrayChunks.Length; i++)
					{
						sizeLength=0;
						dataLength=byteArrayChunks.Span[i].Length;
						while(sizeLength<dataLength)
						{
							if(this.Buffer.Length<=bufferPos)
							{
								await this.Stream.WriteAsync(this.Buffer, this.BufferPos, bufferPos-this.BufferPos, cancellationToken: cancellationToken)
									.ConfigureAwait(false);
								bufferPos=this.BufferPos;

								if(this.Buffer.Length<=bufferPos)
									throw new InternalBufferOverflowException("Cannot write field header, too many nested objects");
							}

							headerLength=Math.Min(dataLength-sizeLength, this.Buffer.Length-bufferPos);
							byteArrayChunks.Span[i].Slice(sizeLength, headerLength).CopyTo(this.Buffer.AsMemory(bufferPos));
							sizeLength+=headerLength;
							bufferPos+=headerLength;
						}
					}
					//Last write
					if(this.BufferPos<bufferPos)
						await this.Stream.WriteAsync(this.Buffer, this.BufferPos, bufferPos-this.BufferPos, cancellationToken: cancellationToken)
							.ConfigureAwait(false);
#else
					//Write object nesting structure
					await this.Stream.WriteAsync(this.Buffer.AsMemory(bufferPos, this.Buffer.Length-bufferPos), cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					//Write all array chunks
					for(i=0; i<byteArrayChunks.Length; i++)
						await this.Stream.WriteAsync(byteArrayChunks.Span[i], cancellationToken: cancellationToken)
							.ConfigureAwait(false);
#endif
					return;
				}
			}

			//If we are here, field header and byte array size was written and there is enough space for whole byte array. Write array itself - all chunks
			this.BufferPos+=headerLength+sizeLength;
			for(i=0; i<byteArrayChunks.Length; i++)
			{
				byteArrayChunks.Span[i].CopyTo(this.Buffer.AsMemory(this.BufferPos));
				this.BufferPos+=byteArrayChunks.Span[i].Length;
			}
		}

		public async ValueTask WriteRawObjectContentAsync(ReadOnlyMemory<byte> byteArray, CancellationToken cancellationToken = default)
		{
			int lengthHoleSize, bufferPos, i;

			//Try write byte array
			if(!byteArray.TryCopyTo(this.Buffer.AsMemory(this.BufferPos)))
			{
				//There was insufficient space in the Buffer. Flush and try again
				await FlushAsync(flushStream: false, cancellationToken: cancellationToken)
					.ConfigureAwait(false);

				//Try again write byte array
				if(!byteArray.TryCopyTo(this.Buffer.AsMemory(this.BufferPos)))
				{
					//Even after flush there was no space to store everything, so use big array strategy
					bufferPos=this.Buffer.Length;

					//Write nested objects structure from right to left
					for(i=this.NestDatasIndex-1; 0<=i; i--)
						WriteFieldHeaderFromRight(this.NestDatas[i].FieldHeader, fieldLength: byteArray.Length, bufferPos: ref bufferPos);

#if NETSTANDARD2_0
					//Write object nesting structure
					await this.Stream.WriteAsync(this.Buffer, bufferPos, this.Buffer.Length-bufferPos, cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					//Write byte array itself
					bufferPos=0;
					while(bufferPos<byteArray.Length)
					{
						//Get size of array chunk
						lengthHoleSize=Math.Min(byteArray.Length-bufferPos, this.Buffer.Length-this.BufferPos);

						byteArray.Slice(bufferPos, lengthHoleSize).CopyTo(this.Buffer.AsMemory(this.BufferPos));

						await this.Stream.WriteAsync(this.Buffer, this.BufferPos, lengthHoleSize, cancellationToken: cancellationToken)
							.ConfigureAwait(false);

						bufferPos+=lengthHoleSize;
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

			//If we are here, byte array was written, actualize BufferPos
			this.BufferPos+=byteArray.Length;
		}

		public async ValueTask WriteRawObjectContentFromChunksAsync(ReadOnlyMemory<ReadOnlyMemory<byte>> byteArrayChunks, CancellationToken cancellationToken = default)
		{
			int headerLength, sizeLength, bufferPos, i, dataLength = 0;

			//Calculate total data size
			for(i=0; i<byteArrayChunks.Length; i++)
				dataLength+=byteArrayChunks.Span[i].Length;

			//Check is there a space in the Buffer to write byte array
			if(this.Buffer.Length<this.BufferPos+dataLength)
			{
				//There was insufficient space in the Buffer. Flush and try again
				await FlushAsync(flushStream: false, cancellationToken: cancellationToken)
					.ConfigureAwait(false);

				//Check again is there a space in the Buffer to write byte array
				if(this.Buffer.Length<this.BufferPos+dataLength)
				{
					//Even after flush there was no space to store everything, so use big array strategy
					bufferPos=this.Buffer.Length;

					//Write nested objects structure from right to left
					for(i=this.NestDatasIndex-1; 0<=i; i--)
						WriteFieldHeaderFromRight(this.NestDatas[i].FieldHeader, fieldLength: dataLength, bufferPos: ref bufferPos);

#if NETSTANDARD2_0
					//Write object nesting structure
					await this.Stream.WriteAsync(this.Buffer, bufferPos, this.Buffer.Length-bufferPos, cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					bufferPos=this.BufferPos;
					//Write all array chunks
					for(i=0; i<byteArrayChunks.Length; i++)
					{
						sizeLength=0;
						dataLength=byteArrayChunks.Span[i].Length;
						while(sizeLength<dataLength)
						{
							if(this.Buffer.Length<=bufferPos)
							{
								await this.Stream.WriteAsync(this.Buffer, this.BufferPos, bufferPos-this.BufferPos, cancellationToken: cancellationToken)
									.ConfigureAwait(false);
								bufferPos=this.BufferPos;

								if(this.Buffer.Length<=bufferPos)
									throw new InternalBufferOverflowException("Cannot write field header, too many nested objects");
							}

							headerLength=Math.Min(dataLength-sizeLength, this.Buffer.Length-bufferPos);
							byteArrayChunks.Span[i].Slice(sizeLength, headerLength).CopyTo(this.Buffer.AsMemory(bufferPos));
							sizeLength+=headerLength;
							bufferPos+=headerLength;
						}
					}
					//Last write
					if(this.BufferPos<bufferPos)
						await this.Stream.WriteAsync(this.Buffer, this.BufferPos, bufferPos-this.BufferPos, cancellationToken: cancellationToken)
							.ConfigureAwait(false);
#else
					//Write object nesting structure
					await this.Stream.WriteAsync(this.Buffer.AsMemory(bufferPos, this.Buffer.Length-bufferPos), cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					//Write all array chunks
					for(i=0; i<byteArrayChunks.Length; i++)
						await this.Stream.WriteAsync(byteArrayChunks.Span[i], cancellationToken: cancellationToken)
							.ConfigureAwait(false);
#endif
					return;
				}
			}

			//If we are here, there is enough space for whole byte array. Write array itself - all chunks
			for(i=0; i<byteArrayChunks.Length; i++)
			{
				byteArrayChunks.Span[i].CopyTo(this.Buffer.AsMemory(this.BufferPos));
				this.BufferPos+=byteArrayChunks.Span[i].Length;
			}
		}

		/// <summary>
		/// Method writes from right field header and size
		/// </summary>
		/// <param name="fieldHeader"></param>
		private void WriteFieldHeaderFromRight(ulong fieldHeader, int fieldLength, ref int bufferPos)
		{
			int headerLength;

			//Write field size
			headerLength=Base128.GetRequiredBytesUInt32((uint)(this.Buffer.Length-bufferPos+fieldLength));
			bufferPos-=headerLength;
			if(bufferPos<this.BufferPos || !Base128.TryWriteUInt32(destination: this.Buffer.AsSpan(bufferPos, headerLength), value: (uint)fieldLength, minBytesToWrite: headerLength, written: out _))
				throw new InternalBufferOverflowException("Cannot write field header, too many nested objects");

			//Write field header
			headerLength=Base128.GetRequiredBytesUInt64(fieldHeader);
			bufferPos-=headerLength;
			if(bufferPos<this.BufferPos || !Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(bufferPos, headerLength), value: fieldHeader, minBytesToWrite: headerLength, written: out _))
				throw new InternalBufferOverflowException("Cannot write field header, too many nested objects");
		}
	}
}