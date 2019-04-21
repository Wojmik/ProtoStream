using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Byte array serializer
	/// </summary>
	public class ByteArraySerializer : SerializerBase<byte[]>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static ByteArraySerializer Default { get; } = new ByteArraySerializer();

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="fieldNo">Unique field no</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public override async ValueTask SerializeAsync(ProtoStreamWriter writer, int fieldNo, byte[] value, CancellationToken cancellationToken = default)
		{
			await writer.WriteBytesArrayAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="previousValue">So far read value</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public override async ValueTask<byte[]> DeserializeAsync(ProtoStreamReader reader, byte[] previousValue, CancellationToken cancellationToken = default)
		{
			int length;
			byte[] value;

			length=reader.CurrentFieldHeader.FieldLength;

			if(previousValue!=null)
			{
				value=new byte[previousValue.Length+length];
				Array.Copy(previousValue, value, previousValue.Length);
			}
			else
				value=new byte[length];

			await reader.ReadBytesArrayAsync(byteArray: new Memory<byte>(value, value.Length-length, length), cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return value;
		}

		/// <summary>
		/// Is <paramref name="value"/> default value
		/// </summary>
		/// <param name="value">Value to check</param>
		/// <returns>True if <paramref name="value"/> is default value, false otherwise</returns>
		public override bool IsDefault(byte[] value)
		{
			return value==default;
		}
	}
}