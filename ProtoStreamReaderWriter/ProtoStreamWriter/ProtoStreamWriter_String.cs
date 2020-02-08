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
			int headerSize, charIndex = 0, lengthHoleSize, charsUsed, bytesUsed;
			ulong fieldHeader;
			bool completed;
#if NETSTANDARD2_0
			int chunkBytesUsed;
			bool flush;
#endif

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
					charsUsed=value.Length-charIndex;
					flush=charsUsed<=this.CharBuffer.Length;
					if(!flush)
						charsUsed=this.CharBuffer.Length;

					value.Slice(charIndex, charsUsed).CopyTo(this.CharBuffer);

					this.StringEncoder.Convert(this.CharBuffer, 0, charsUsed, this.Buffer, this.BufferPos+bytesUsed, this.Buffer.Length-this.BufferPos-bytesUsed, flush, out charsUsed, out chunkBytesUsed, out completed);
					charIndex+=charsUsed;
					bytesUsed+=chunkBytesUsed;
				}
				while(completed && !flush);
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

		public async ValueTask WriteStringWithoutFragmentationAsync(int fieldNo, string value, CancellationToken cancellationToken = default)
		{
			if(value!=null)
			{
				await WriteStringWithoutFragmentationAsync(fieldNo: fieldNo, value: value.AsMemory(), getAccurateStringByteLength: GetAccurateStringByteLength, cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}

			int GetAccurateStringByteLength()
			{
				return this.StringEncoding.GetByteCount(value);
			}
		}

		public async ValueTask WriteStringWithoutFragmentationAsync(int fieldNo, string value, int charIndex, int charCount, CancellationToken cancellationToken = default)
		{
			if(value==null)
				throw new ArgumentNullException(nameof(value));

			await WriteStringWithoutFragmentationAsync(fieldNo: fieldNo, value: value.AsMemory(charIndex, charCount), getAccurateStringByteLength: GetAccurateStringByteLength, cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			int GetAccurateStringByteLength()
			{
#if NETSTANDARD2_0
				int charPos = 0, bytes = 0, charsUsed;
				bool flush;

				while(0<(charsUsed=charCount-charPos))
				{
					flush=charsUsed<=this.CharBuffer.Length;
					if(!flush)
						charsUsed=this.CharBuffer.Length;
					//What a waste 😟
					value.CopyTo(charIndex+charPos, this.CharBuffer, 0, charsUsed);

					bytes+=this.StringEncoder.GetByteCount(this.CharBuffer, 0, charsUsed, flush);
					charPos+=charsUsed;
				}
				return bytes;
#else
				return this.StringEncoding.GetByteCount(value, charIndex, charCount);
#endif
			}
		}

		public async ValueTask WriteStringWithoutFragmentationAsync(int fieldNo, ReadOnlyMemory<char> value, CancellationToken cancellationToken = default)
		{
			await WriteStringWithoutFragmentationAsync(fieldNo: fieldNo, value: value, getAccurateStringByteLength: GetAccurateStringByteLength, cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			int GetAccurateStringByteLength()
			{
#if NETSTANDARD2_0
				int charPos = 0, bytes = 0, charsUsed;
				bool flush;

				while(0<(charsUsed=value.Length-charPos))
				{
					flush=charsUsed<=this.CharBuffer.Length;
					if(!flush)
						charsUsed=this.CharBuffer.Length;
					//What a waste 😟
					value.Slice(charPos, charsUsed).CopyTo(this.CharBuffer);

					bytes+=this.StringEncoder.GetByteCount(this.CharBuffer, 0, charsUsed, flush);
					charPos+=charsUsed;
				}
				return bytes;
#else
				return this.StringEncoding.GetByteCount(value.Span);
#endif
			}
		}

		private async ValueTask WriteStringWithoutFragmentationAsync(int fieldNo, ReadOnlyMemory<char> value, Func<int> getAccurateStringByteLength, CancellationToken cancellationToken = default)
		{
			int headerSize, charIndex = 0, lengthHoleSize, charsUsed, bytesUsed, maxLength, bufferPos, i;
			ulong fieldHeader;
			bool completed = false;
#if NETSTANDARD2_0
			int chunkBytesUsed;
			bool flush;
#endif

			fieldHeader=CalculateFieldHeader(fieldNo: fieldNo, wireType: WireType.LengthDelimited);
			maxLength=this.StringEncoding.GetMaxByteCount(value.Length-charIndex);
			lengthHoleSize=Base128.GetRequiredBytesUInt32((uint)maxLength);
			this.StringEncoder.Reset();

			//Try write field header and check is there a space for string size and string itself
			if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerSize)
				|| this.Buffer.Length-this.BufferPos<headerSize+lengthHoleSize+maxLength)
			{
				//There was insufficient space in the Buffer. Flush and try again
				await FlushAsync(flushStream: false, cancellationToken: cancellationToken)
					.ConfigureAwait(false);

				//Try again write field header and check is there a space for string size and string itself
				if(!Base128.TryWriteUInt64(destination: this.Buffer.AsSpan(this.BufferPos), value: fieldHeader, written: out headerSize)
					|| this.Buffer.Length-this.BufferPos<headerSize+lengthHoleSize+maxLength)
				{
					//Even after flush there was no space to store everything, so use big string strategy
					maxLength=getAccurateStringByteLength();

					bufferPos=this.Buffer.Length;

					//Write string header from right to left
					WriteFieldHeaderFromRight(fieldHeader, fieldLength: maxLength, bufferPos: ref bufferPos);

					//Write nested objects structure from right to left
					for(i=this.NestDatasIndex-1; 0<=i; i--)
						WriteFieldHeaderFromRight(this.NestDatas[i].FieldHeader, fieldLength: maxLength, bufferPos: ref bufferPos);

#if NETSTANDARD2_0
					//Write object nesting structure
					await this.Stream.WriteAsync(this.Buffer, bufferPos, this.Buffer.Length-bufferPos, cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					//Write string itself
					while(charIndex<value.Length && !completed)
					{
						bufferPos=0;
						do
						{
							//Copy chunk to CharBuffer
							charsUsed=value.Length-charIndex;
							flush=charsUsed<=this.CharBuffer.Length;
							if(!flush)
								charsUsed=this.CharBuffer.Length;
							value.Slice(charIndex, charsUsed).CopyTo(this.CharBuffer);

							this.StringEncoder.Convert(this.CharBuffer, 0, charsUsed, this.Buffer, this.BufferPos+bufferPos, this.Buffer.Length-this.BufferPos-bufferPos, flush, out charsUsed, out bytesUsed, out completed);
							charIndex+=charsUsed;
							bufferPos+=bytesUsed;
						}
						while(completed && !flush);

						await this.Stream.WriteAsync(this.Buffer, this.BufferPos, bufferPos, cancellationToken: cancellationToken)
							.ConfigureAwait(false);
					}
#else
					//Write object nesting structure
					await this.Stream.WriteAsync(this.Buffer.AsMemory(bufferPos, this.Buffer.Length-bufferPos), cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					//Write string itself
					while(charIndex<value.Length && !completed)
					{
						this.StringEncoder.Convert(value.Span.Slice(charIndex), this.Buffer.AsSpan(this.BufferPos), true, out charsUsed, out bytesUsed, out completed);
						charIndex+=charsUsed;

						await this.Stream.WriteAsync(this.Buffer.AsMemory(this.BufferPos, bytesUsed), cancellationToken: cancellationToken)
							.ConfigureAwait(false);
					}
#endif
					return;
				}
			}

			//If we are here, field header was written, space for field size reserved and string itself will fit into the Buffer
			this.BufferPos+=headerSize+lengthHoleSize;

#if NETSTANDARD2_0
			bytesUsed=0;
			do
			{
				//Calculate how many chars copy to CharBuffer and is it last chunk
				charsUsed=value.Length-charIndex;//headerSize is now chars to convert
				flush=charsUsed<=this.CharBuffer.Length;
				if(!flush)
					charsUsed=this.CharBuffer.Length;

				value.Slice(charIndex, charsUsed).CopyTo(this.CharBuffer);

				this.StringEncoder.Convert(this.CharBuffer, 0, charsUsed, this.Buffer, this.BufferPos+bytesUsed, this.Buffer.Length-this.BufferPos-bytesUsed, flush, out charsUsed, out chunkBytesUsed, out completed);
				charIndex+=charsUsed;
				bytesUsed+=chunkBytesUsed;
			}
			while(completed && !flush);
#else
			this.StringEncoder.Convert(value.Span, this.Buffer.AsSpan(this.BufferPos), true, out charsUsed, out bytesUsed, out completed);
			charIndex+=charsUsed;
#endif
			if(!completed)//Should always be true
				throw new InternalBufferOverflowException("Cannot write string without fragmentation, protocol error");

			//Save length in bytes of written string
			Base128.WriteUInt32(destination: this.Buffer.AsSpan(this.BufferPos-lengthHoleSize, lengthHoleSize), value: (uint)bytesUsed, minBytesToWrite: lengthHoleSize, written: out _);
			this.BufferPos+=bytesUsed;
		}
	}
}