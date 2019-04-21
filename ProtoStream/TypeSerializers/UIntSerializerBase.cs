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
	/// Base serializer for uint fields
	/// </summary>
	public abstract class UIntSerializerBase : SerializerValueBase<uint>
	{
		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="previousValue">So far read value</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public override async ValueTask<uint> DeserializeAsync(ProtoStreamReader reader, uint previousValue, CancellationToken cancellationToken = default)
		{
			WireType wireType;
			uint value;

			wireType=reader.CurrentFieldHeader.WireType;
			if(wireType==Internal.WireType.VarInt)
				value=(uint)await reader.ReadVarUIntAsync(cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			else if(wireType==Internal.WireType.Fixed32)
				value=(uint)await reader.ReadInt32Async(cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			else if(wireType==Internal.WireType.Fixed64)
				value=(uint)await reader.ReadInt64Async(cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			else
				throw new SerializationException($"Unexpected wire type: {wireType} for type: {typeof(uint)}");

			return value;
		}

		/// <summary>
		/// Is <paramref name="value"/> default value
		/// </summary>
		/// <param name="value">Value to check</param>
		/// <returns>True if <paramref name="value"/> is default value, false otherwise</returns>
		public override bool IsDefault(uint value)
		{
			return value==default;
		}
	}
}