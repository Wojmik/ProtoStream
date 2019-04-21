using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Base serializer for type T with serializing value only - without header
	/// </summary>
	/// <typeparam name="T">Type</typeparam>
	public abstract class SerializerValueBase<T> : SerializerBase<T>, ISerializerValue<T>
	{
		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public abstract ValueTask SerializeValueAsync(ProtoStreamWriter writer, T value, CancellationToken cancellationToken = default);

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public abstract ValueTask<ValueWithSize<T>> DeserializeValueAsync(ProtoStreamReader reader, CancellationToken cancellationToken = default);
	}
}