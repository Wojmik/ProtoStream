using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter
{
	partial class ProtoStreamWriter
	{
		public async ValueTask WriteStringAsync(int fieldNo, string value, CancellationToken cancellationToken = default)
		{
			if(value!=null)
			{
				await WriteStringAsync(fieldNo: fieldNo, value: value.AsMemory(), cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}
		}

		public async ValueTask WriteStringAsync(int fieldNo, string value, int charIndex, int charCount, CancellationToken cancellationToken = default)
		{
			if(value==null)
				throw new ArgumentNullException(nameof(value));

			await WriteStringAsync(fieldNo: fieldNo, value: value.AsMemory(charIndex, charCount), cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		public async ValueTask WriteStringAsync(int fieldNo, ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
		{
			int headerSize, charIndex = 0, lengthHoleSize, charsUsed, bytesUsed, chunkBytesUsed;
			ulong fieldHeader;
			bool completed;

			fieldHeader=CalculateFieldHeader(fieldNo: fieldNo, wireType: WireType.LengthDelimited);
			this.StringEncoder.Reset();

			do
			{
				//Try write field header and reserve space for field size
				if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerSize)
					|| this.Buffer.Length-this.BufferPos-headerSize<(lengthHoleSize=Base128.GetRequiredBytesUInt32((uint)Math.Min(this.StringEncoding.GetMaxByteCount(value.Length-charIndex), this.Buffer.Length-this.BufferPos-headerSize))))
				{
					//There was insufficient space in the Buffer. Flush and try again
					await FlushAsync(flushStream: false, cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					//Try again write field header and reserve space for field size
					if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerSize)
						|| this.Buffer.Length-this.BufferPos-headerSize<(lengthHoleSize=Base128.GetRequiredBytesUInt32((uint)Math.Min(this.StringEncoding.GetMaxByteCount(value.Length-charIndex), this.Buffer.Length-this.BufferPos-headerSize))))
						throw new InternalBufferOverflowException("Cannot write field, too many nested objects");
				}

				//If we are here, field header was written and space for field size reserved, actualize BufferPos
				this.BufferPos+=headerSize+lengthHoleSize;

#if NETSTANDARD2_0
				bytesUsed=0;
				do
				{
					//Calculate how many chars copy to CharBuffer and is it last chunk
					headerSize=value.Length-charIndex;//headerSize is now chars to convert
					completed=headerSize<=this.CharBuffer.Length;
					if(!completed)
						headerSize=this.CharBuffer.Length;

					value.Slice(charIndex, headerSize).CopyTo(this.CharBuffer);

					this.StringEncoder.Convert(this.CharBuffer, 0, headerSize, this.Buffer, this.BufferPos+bytesUsed, this.Buffer.Length-this.BufferPos-bytesUsed, completed, out charsUsed, out chunkBytesUsed, out completed);
					charIndex+=charsUsed;
					bytesUsed+=chunkBytesUsed;
				}
				while(!completed && charsUsed>=headerSize);
#else
				this.StringEncoder.Convert(value.Span.Slice(charIndex), this.Buffer.AsSpan(this.BufferPos), true, out charsUsed, out bytesUsed, out completed);
				charIndex+=charsUsed;
#endif

				//Save length in bytes of written string chunk
				Base128.WriteUInt32(destination: this.Buffer.AsSpan(this.BufferPos-lengthHoleSize, lengthHoleSize), value: (uint)bytesUsed, minBytesToWrite: lengthHoleSize, written: out _);
				this.BufferPos+=bytesUsed;
			}
			while(!completed);
		}
	}
}