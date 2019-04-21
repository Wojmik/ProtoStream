using ProtoStream.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for Nullable&lt;<typeparamref name="TInner"/>&gt;
	/// </summary>
	/// <typeparam name="TInner">Type of Nullable inner field</typeparam>
	public class NullableSerializer<TInner> : SerializerBase<Nullable<TInner>>
		where TInner : struct
	{
		/// <summary>
		/// Get maximum size of item
		/// </summary>
		public override int MaxSize { get => InnerSerializer.MaxSize; }

		/// <summary>
		/// Inner serializer
		/// </summary>
		protected SerializerBase<TInner> InnerSerializer { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="innerSerializer">Inner serializer</param>
		public NullableSerializer(SerializerBase<TInner> innerSerializer)
		{
			this.InnerSerializer=innerSerializer;
		}

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="fieldNo">Unique field no</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public override async ValueTask SerializeAsync(ProtoStreamWriter writer, int fieldNo, Nullable<TInner> value, CancellationToken cancellationToken = default)
		{
			if(value.HasValue)
				await InnerSerializer.SerializeAsync(writer: writer, fieldNo: fieldNo, value: value.Value, cancellationToken: cancellationToken)
					.ConfigureAwait(false);
		}

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="previousValue">So far read value</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public override async ValueTask<Nullable<TInner>> DeserializeAsync(ProtoStreamReader reader, Nullable<TInner> previousValue, CancellationToken cancellationToken = default)
		{
			return await InnerSerializer.DeserializeAsync(reader: reader, previousValue: previousValue.HasValue ? previousValue.Value : default(TInner), cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Is <paramref name="value"/> default value
		/// </summary>
		/// <param name="value">Value to check</param>
		/// <returns>True if <paramref name="value"/> is default value, false otherwise</returns>
		public override bool IsDefault(Nullable<TInner> value)
		{
			return !value.HasValue;
		}
	}
}