using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoStream
{
	/// <summary>
	/// Support object value serializing / deserializing
	/// </summary>
	/// <typeparam name="T">Type of object</typeparam>
	public interface ISerializerValue<T> :ISerializer<T>
	{
		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		ValueTask SerializeValueAsync(ProtoStreamWriter writer, T value, CancellationToken cancellationToken);

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		ValueTask<ValueWithSize<T>> DeserializeValueAsync(ProtoStreamReader reader, CancellationToken cancellationToken);
	}
}