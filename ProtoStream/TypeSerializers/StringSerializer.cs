using ProtoStream.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// String serializer
	/// </summary>
	public class StringSerializer : SerializerBase<string>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static StringSerializer Default { get; } = new StringSerializer();

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="fieldNo">Unique field no</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public override async ValueTask SerializeAsync(ProtoStreamWriter writer, int fieldNo, string value, CancellationToken cancellationToken = default)
		{
			await writer.WriteStringAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="previousValue">So far read value</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public override async ValueTask<string> DeserializeAsync(ProtoStreamReader reader, string previousValue, CancellationToken cancellationToken = default)
		{
			string value;

			value=await reader.ReadStringAsync(cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			if(previousValue!=null)
				value=previousValue+value;

			return value;
		}

		/// <summary>
		/// Is <paramref name="value"/> default value
		/// </summary>
		/// <param name="value">Value to check</param>
		/// <returns>True if <paramref name="value"/> is default value, false otherwise</returns>
		public override bool IsDefault(string value)
		{
			return value==default;
		}
	}
}