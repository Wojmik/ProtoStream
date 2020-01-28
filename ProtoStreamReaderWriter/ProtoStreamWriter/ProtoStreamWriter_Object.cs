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
		public async ValueTask ObjectEnterAsync(int fieldNo, CancellationToken cancellationToken = default)
		{
			int headerLength, sizeLength;
			ulong fieldHeader;

			fieldHeader=CalculateFieldHeader(fieldNo: fieldNo, wireType: WireType.LengthDelimited);
			//Try write field header and reserve place for object size
			if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerLength)
				|| this.Buffer.Length-this.BufferPos-headerLength<(sizeLength=Base128.GetRequiredBytesUInt32((uint)(this.Buffer.Length-this.BufferPos-headerLength))))
			{
				//There was insufficient space in the Buffer. Flush and try again
				await FlushAsync(flushStream: false, cancellationToken: cancellationToken)
					.ConfigureAwait(false);

				//Try again write field header and reserve place for object size
				if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerLength)
					|| this.Buffer.Length-this.BufferPos-headerLength<(sizeLength=Base128.GetRequiredBytesUInt32((uint)(this.Buffer.Length-this.BufferPos-headerLength))))
					throw new InternalBufferOverflowException("Cannot write field header, too many nested objects");
			}

			//If we are here, field header was written and space for object size reserved. Save NestData object
			this.NestDatasIndex++;
			if(this.NestDatas.Length<=this.NestDatasIndex)
			{
				//NestDatas array is too small, allocate bigger
				NestData[] newNestData = new NestData[this.NestDatas.Length<<1];
				Array.Copy(this.NestDatas, newNestData, this.NestDatas.Length);
				this.NestDatas=newNestData;
			}
			this.BufferPos+=headerLength+sizeLength;
			this.NestDatas[this.NestDatasIndex]=new NestData(fieldHeader: fieldHeader, sizeSpaceLength: sizeLength, dataStartIndex: this.BufferPos);
		}

		public void ObjectLeave()
		{
			if(this.NestDatasIndex<0)
				throw new InvalidOperationException("Leaving not entered object");

			ref NestData nestData = ref this.NestDatas[this.NestDatasIndex];

			//Try write living object size in space left
			if(!Base128.TryWriteUInt32(destination: this.Buffer.AsSpan(nestData.DataStartIndex-nestData.SizeSpaceLength, nestData.SizeSpaceLength), value: (uint)(this.BufferPos-nestData.DataStartIndex), minBytesToWrite: nestData.SizeSpaceLength, written: out _))
				throw new Exception("Space left for object length is too small. Protocol error");

			this.NestDatasIndex--;
		}
	}
}