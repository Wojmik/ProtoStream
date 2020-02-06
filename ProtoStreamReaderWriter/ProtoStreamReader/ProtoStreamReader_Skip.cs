using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WojciechMikołajewicz.ProtoStreamReaderWriter.InternalModel;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter
{
	partial class ProtoStreamReader
	{
		#region Method tables
		private static readonly SkipAsyncHandler[] SkipMethods = new SkipAsyncHandler[]
		{
			SkipVarInt64Async,//VarInt = 0
			SkipFixedInt64Async,//Fixed64 = 1
			SkipLengthDelimitedAsync,//LengthDelimited = 2
			SkipUnsuportedAsync,//3
			SkipUnsuportedAsync,//4
			SkipFixedInt32Async,//Fixed32 = 5
			SkipUnsuportedAsync,//6
			SkipUnsuportedAsync,//7
		};
		#endregion

		public async ValueTask SkipFieldAsync(WireType wireType, CancellationToken cancellationToken = default)
		{
			await SkipMethods[(int)wireType](psr: this, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		private static async ValueTask SkipVarInt64Async(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			int read;

			if(!Base128.TrySkip(source: psr.Buffer.AsSpan(psr.BufferPos, psr.BufferPopulatedLength-psr.BufferPos), read: out read))
			{
				if(!await psr.PopulateVarIntAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream. Cannot read VarInt value.");

				if(!Base128.TrySkip(source: psr.Buffer.AsSpan(psr.BufferPos, psr.BufferPopulatedLength-psr.BufferPos), read: out read))
					throw new Exception("Cannot read VarInt value. Protocol error");
			}
			psr.BufferPos+=read;
		}

		private static async ValueTask SkipFixedInt64Async(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			if(psr.BufferPopulatedLength<psr.BufferPos+sizeof(long))
			{
				if(!await psr.PopulateFixedAsync(length: sizeof(long), cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream. Cannot read uint value.");

				if(psr.BufferPopulatedLength<psr.BufferPos+sizeof(long))
					throw new Exception("Cannot read long value. Protocol error");
			}
			psr.BufferPos+=sizeof(long);
		}

		private static async ValueTask SkipFixedInt32Async(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			if(psr.BufferPopulatedLength<psr.BufferPos+sizeof(int))
			{
				if(!await psr.PopulateFixedAsync(length: sizeof(int), cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream. Cannot read uint value.");

				if(psr.BufferPopulatedLength<psr.BufferPos+sizeof(int))
					throw new Exception("Cannot read int value. Protocol error");
			}
			psr.BufferPos+=sizeof(int);
		}

		private static async ValueTask SkipLengthDelimitedAsync(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			ulong endObjectPosition;
			int toRead;

			if(psr.NestDatasIndex<0)
				throw new Exception("Not in variable length field. Cannot skip variable lenth field. Protocol error");

			endObjectPosition=psr.NestDatas[psr.NestDatasIndex].EndObjectPosition;

			while(true)
			{
				psr.BufferPos+=Math.Min(psr.BufferPopulatedLength-psr.BufferPos, (int)(endObjectPosition-psr.ShrinkedBufferLength)-psr.BufferPos);

				toRead=(int)(endObjectPosition-psr.ShrinkedBufferLength)-psr.BufferPos;
				if(toRead<=0)
					break;

				if(!await psr.PopulateFixedAsync(length: toRead, cancellationToken: cancellationToken).ConfigureAwait(false))
					throw new EndOfStreamException("Unexpected end of stream. Cannot skip variable length area.");
			}

			psr.NestDatasIndex--;
		}

		private static ValueTask SkipUnsuportedAsync(ProtoStreamReader psr, CancellationToken cancellationToken)
		{
			throw new NotSupportedException("Unsuported wire type while deserializing");
		}
	}
}