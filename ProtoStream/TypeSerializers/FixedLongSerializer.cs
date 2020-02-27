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
	public class FixedLongSerializer : LongSerializerBase
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static FixedLongSerializer Default { get; } = new FixedLongSerializer();

		/// <summary>
		/// Get maximum size of item
		/// </summary>
		public override int MaxSize { get => sizeof(long); }

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="fieldNo">Unique field no</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public override async ValueTask SerializeAsync(ProtoStreamWriter writer, int fieldNo, long value, CancellationToken cancellationToken = default)
		{
			await writer.WriteInt64Async(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public override async ValueTask SerializeValueAsync(ProtoStreamWriter writer, long value, CancellationToken cancellationToken = default)
		{
			await writer.WriteInt64ValueAsync(value: value, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public override async ValueTask<ValueWithSize<long>> DeserializeValueAsync(ProtoStreamReader reader, CancellationToken cancellationToken = default)
		{
			return new ValueWithSize<long>(value: await reader.ReadInt64Async(cancellationToken: cancellationToken).ConfigureAwait(false), size: sizeof(long));
		}
	}
}