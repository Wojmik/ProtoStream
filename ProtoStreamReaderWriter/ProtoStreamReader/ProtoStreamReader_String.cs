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
		public async ValueTask<string> ReadStringAsync(string previousValue, CancellationToken cancellationToken = default)
		{
			ulong endObjectPosition;
			int toRead, charIndex = 0, charsUsed;
			bool finish, completed;

			if(this.NestDatasIndex<0)
				throw new Exception("Not in variable length field. Cannot read string. Protocol error");

			endObjectPosition=this.NestDatas[this.NestDatasIndex].EndObjectPosition;

			toRead=(int)(endObjectPosition-this.ShrinkedBufferLength)-this.BufferPos;
			charsUsed=this.StringEncoding.GetMaxCharCount(toRead);
			//Ensure CharBuffer size is enough
			if(this.CharBuffer.Length<charsUsed)
			{
				System.Buffers.ArrayPool<char>.Shared.Return(Interlocked.Exchange(ref this.CharBuffer, System.Buffers.ArrayPool<char>.Shared.Rent(charsUsed)), true);
			}

			this.StringDecoder.Reset();

			while(true)
			{
				finish=toRead<=this.BufferPopulatedLength-this.BufferPos;
				if(!finish)
					toRead=this.BufferPopulatedLength-this.BufferPos;
				this.StringDecoder.Convert(this.Buffer, this.BufferPos, toRead, this.CharBuffer, charIndex, this.CharBuffer.Length-charIndex, finish, out toRead, out charsUsed, out completed);
				this.BufferPos+=toRead;
				charIndex+=charsUsed;

				if(finish)
				{
					if(!completed)
						throw new Exception("Cannot complete string decoding");
					break;
				}

				toRead=(int)(endObjectPosition-this.ShrinkedBufferLength)-this.BufferPos;

				if(!await this.PopulateFixedAsync(length: toRead, cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream. Cannot read whole string.");
			}

			this.NestDatasIndex--;

			if(previousValue!=null)
			{
#if NETSTANDARD2_0
				previousValue+=new string(this.CharBuffer, 0, charIndex);
#else
				previousValue=string.Create(previousValue.Length+charIndex, (Old: previousValue, Current: this.CharBuffer.AsMemory(0, charIndex)), (span, state) =>
				{
					state.Old.AsSpan().CopyTo(span);
					state.Current.Span.CopyTo(span.Slice(state.Old.Length));
				});
#endif
			}
			else
				previousValue=new string(this.CharBuffer, 0, charIndex);

			return previousValue;
		}
	}
}