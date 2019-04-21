using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoStream
{
	/// <summary>
	/// Support object serializing / deserializing
	/// </summary>
	/// <typeparam name="T">Type of object</typeparam>
	public interface ISerializer<T>
	{
		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="fieldNo">Unique field no</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		ValueTask SerializeAsync(ProtoStreamWriter writer, int fieldNo, T value, CancellationToken cancellationToken);

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="previousValue">So far read value</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		ValueTask<T> DeserializeAsync(ProtoStreamReader reader, T previousValue, CancellationToken cancellationToken);

		/// <summary>
		/// Is <paramref name="value"/> default for the type <typeparamref name="T"/>
		/// </summary>
		/// <param name="value">Value to check</param>
		/// <returns>True if <paramref name="value"/> is default value for type <typeparamref name="T"/>, false otherwise</returns>
		bool IsDefault(T value);
	}
}