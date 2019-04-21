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
	/// Serializer for sbyte fields serialized as VarInt
	/// </summary>
	public class VarSByteSerializer : SerializerValueBase<sbyte>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static VarSByteSerializer Default { get; } = new VarSByteSerializer();

		/// <summary>
		/// Get maximum size of item
		/// </summary>
		public override int MaxSize { get => 2; }

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="fieldNo">Unique field no</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public override async ValueTask SerializeAsync(ProtoStreamWriter writer, int fieldNo, sbyte value, CancellationToken cancellationToken = default)
		{
			await writer.WriteVarIntAsync(fieldNo: fieldNo, value: (long)value, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="previousValue">So far read value</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public override async ValueTask<sbyte> DeserializeAsync(ProtoStreamReader reader, sbyte previousValue, CancellationToken cancellationToken = default)
		{
			WireType wireType;
			sbyte value;

			wireType=reader.CurrentFieldHeader.WireType;
			if(wireType==Internal.WireType.VarInt)
				value=(sbyte)await reader.ReadVarIntAsync(cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			else if(wireType==Internal.WireType.Fixed32)
				value=(sbyte)await reader.ReadInt32Async(cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			else if(wireType==Internal.WireType.Fixed64)
				value=(sbyte)await reader.ReadInt64Async(cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			else
				throw new SerializationException($"Unexpected wire type: {wireType} for type: {typeof(sbyte)}");

			return value;
		}

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public override async ValueTask SerializeValueAsync(ProtoStreamWriter writer, sbyte value, CancellationToken cancellationToken = default)
		{
			await writer.WriteVarIntValueAsync(value: (long)value, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public override async ValueTask<ValueWithSize<sbyte>> DeserializeValueAsync(ProtoStreamReader reader, CancellationToken cancellationToken = default)
		{
			ValueWithSize<long> value;

			value=await reader.ReadVarIntWithSizeAsync(cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return new ValueWithSize<sbyte>(value: (sbyte)value.Value, size: value.Size);
		}

		/// <summary>
		/// Is <paramref name="value"/> default value
		/// </summary>
		/// <param name="value">Value to check</param>
		/// <returns>True if <paramref name="value"/> is default value, false otherwise</returns>
		public override bool IsDefault(sbyte value)
		{
			return value==default;
		}
	}
}