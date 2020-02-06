using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter
{
	partial class ProtoStreamReader
	{
		public async ValueTask<byte[]> ReadByteArrayAsync(byte[] previousValue, CancellationToken cancellationToken)
		{
			ulong endObjectPosition;
			int toRead, chunk;
			byte[] array;

			if(this.NestDatasIndex<0)
				throw new Exception("Not in variable length field. Cannot read byte array. Protocol error");

			endObjectPosition=this.NestDatas[this.NestDatasIndex].EndObjectPosition;

			toRead=(int)(endObjectPosition-this.ShrinkedBufferLength)-this.BufferPos;

			array=new byte[(previousValue!=null ? previousValue.Length : 0)+toRead];

			while(true)
			{
				chunk=Math.Min(this.BufferPopulatedLength-this.BufferPos, toRead);
				Array.Copy(this.Buffer, this.BufferPos, array, array.Length-toRead, chunk);
				this.BufferPos+=chunk;
				toRead-=chunk;
				if(toRead<=0)
					break;

				if(!await this.PopulateFixedAsync(length: toRead, cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream. Cannot read whole byte array.");
			}

			this.NestDatasIndex--;

			//Copy previous array to the begining of new array
			if(previousValue!=null)
				Array.Copy(previousValue, array, previousValue.Length);

			return array;
		}
	}
}