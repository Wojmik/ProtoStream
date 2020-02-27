using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for int fields serialized as Fixed
	/// </summary>
	public class FixedIntSerializer : IntSerializerBase
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static FixedIntSerializer Default { get; } = new FixedIntSerializer();

		/// <summary>
		/// Get maximum size of item
		/// </summary>
		public override int MaxSize { get => sizeof(int); }

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
			await writer.WriteInt32Async(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken)
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
			await writer.WriteInt32ValueAsync(value: value, cancellationToken: cancellationToken)
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
			return new ValueWithSize<int>(value: await reader.ReadInt32Async(cancellationToken: cancellationToken).ConfigureAwait(false), size: sizeof(int));
		}
	}
}