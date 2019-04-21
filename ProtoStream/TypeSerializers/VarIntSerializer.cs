using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for int fields serialized as VarInt
	/// </summary>
	public class VarIntSerializer : IntSerializerBase
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static VarIntSerializer Default { get; } = new VarIntSerializer();

		/// <summary>
		/// Get maximum size of item
		/// </summary>
		public override int MaxSize { get => 5; }

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="fieldNo">Unique field no</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public override async ValueTask SerializeAsync(ProtoStreamWriter writer, int fieldNo, int value, CancellationToken cancellationToken = default)
		{
			await writer.WriteVarIntAsync(fieldNo: fieldNo, value: (long)value, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public override async ValueTask SerializeValueAsync(ProtoStreamWriter writer, int value, CancellationToken cancellationToken = default)
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
		public override async ValueTask<ValueWithSize<int>> DeserializeValueAsync(ProtoStreamReader reader, CancellationToken cancellationToken = default)
		{
			ValueWithSize<long> value;

			value=await reader.ReadVarIntWithSizeAsync(cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return new ValueWithSize<int>(value: (int)value.Value, size: value.Size);
		}
	}
}