using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProtoStream.Internal;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Base serializer for long fields
	/// </summary>
	public abstract class LongSerializerBase : SerializerValueBase<long>
	{
		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="previousValue">So far read value</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public override async ValueTask<long> DeserializeAsync(ProtoStreamReader reader, long previousValue, CancellationToken cancellationToken = default)
		{
			WireType wireType;
			long value;

			wireType=reader.CurrentFieldHeader.WireType;
			if(wireType==Internal.WireType.VarInt)
				value=await reader.ReadVarIntAsync(cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			else if(wireType==Internal.WireType.Fixed64)
				value=await reader.ReadInt64Async(cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			else if(wireType==Internal.WireType.Fixed32)
				value=await reader.ReadInt32Async(cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			else
				throw new SerializationException($"Unexpected wire type: {wireType} for type: {typeof(long)}");

			return value;
		}

		/// <summary>
		/// Is <paramref name="value"/> default value
		/// </summary>
		/// <param name="value">Value to check</param>
		/// <returns>True if <paramref name="value"/> is default value, false otherwise</returns>
		public override bool IsDefault(long value)
		{
			return value==default;
		}
	}
}