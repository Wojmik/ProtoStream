using ProtoStream.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for bool fields serialized as VarInt
	/// </summary>
	public class BoolSerializer : SerializerValueBase<bool>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static BoolSerializer Default { get; } = new BoolSerializer();

		/// <summary>
		/// Get maximum size of item
		/// </summary>
		public override int MaxSize { get => 1; }

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="fieldNo">Unique field no</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public override async ValueTask SerializeAsync(ProtoStreamWriter writer, int fieldNo, bool value, CancellationToken cancellationToken = default)
		{
			await writer.WriteBoolAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="previousValue">So far read value</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public override async ValueTask<bool> DeserializeAsync(ProtoStreamReader reader, bool previousValue, CancellationToken cancellationToken = default)
		{
			WireType wireType;
			ulong value;

			wireType=reader.CurrentFieldHeader.WireType;
			if(wireType==Internal.WireType.VarInt)
				value=await reader.ReadVarUIntAsync(cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			else if(wireType==Internal.WireType.Fixed32)
				value=(ulong)await reader.ReadInt32Async(cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			else if(wireType==Internal.WireType.Fixed64)
				value=(ulong)await reader.ReadInt64Async(cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			else
				throw new SerializationException($"Unexpected wire type: {wireType} for type: {typeof(bool)}");

			return value!=0UL;
		}

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public override async ValueTask SerializeValueAsync(ProtoStreamWriter writer, bool value, CancellationToken cancellationToken = default)
		{
			await writer.WriteBoolValueAsync(value: value, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public override async ValueTask<ValueWithSize<bool>> DeserializeValueAsync(ProtoStreamReader reader, CancellationToken cancellationToken = default)
		{
			ValueWithSize<ulong> value;

			value=await reader.ReadVarUIntWithSizeAsync(cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return new ValueWithSize<bool>(value: value.Value!=0UL, size: value.Size);
		}

		/// <summary>
		/// Is <paramref name="value"/> default value
		/// </summary>
		/// <param name="value">Value to check</param>
		/// <returns>True if <paramref name="value"/> is default value, false otherwise</returns>
		public override bool IsDefault(bool value)
		{
			return value==default;
		}
	}
}