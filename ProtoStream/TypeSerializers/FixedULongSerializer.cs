using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for ulong fields serialized as Fixed
	/// </summary>
	public class FixedULongSerializer : ULongSerializerBase
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static FixedULongSerializer Default { get; } = new FixedULongSerializer();

		/// <summary>
		/// Get maximum size of item
		/// </summary>
		public override int MaxSize { get => sizeof(ulong); }

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="fieldNo">Unique field no</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public override async ValueTask SerializeAsync(ProtoStreamWriter writer, int fieldNo, ulong value, CancellationToken cancellationToken = default)
		{
			await writer.WriteInt64Async(fieldNo: fieldNo, value: (long)value, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public override async ValueTask SerializeValueAsync(ProtoStreamWriter writer, ulong value, CancellationToken cancellationToken = default)
		{
			await writer.WriteInt64ValueAsync(value: (long)value, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public override async ValueTask<ValueWithSize<ulong>> DeserializeValueAsync(ProtoStreamReader reader, CancellationToken cancellationToken = default)
		{
			return new ValueWithSize<ulong>(value: (ulong)await reader.ReadInt64Async(cancellationToken: cancellationToken).ConfigureAwait(false), size: sizeof(ulong));
		}
	}
}