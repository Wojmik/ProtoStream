using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for decimal fields
	/// </summary>
	public class DecimalSerializer : SerializerBase<decimal>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static DecimalSerializer Default { get; } = new DecimalSerializer();

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="fieldNo">Unique field no</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public override async ValueTask SerializeAsync(ProtoStreamWriter writer, int fieldNo, decimal value, CancellationToken cancellationToken = default)
		{
			await writer.WriteDecimalAsync(fieldNo: fieldNo, value: value, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="previousValue">So far read value</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public override async ValueTask<decimal> DeserializeAsync(ProtoStreamReader reader, decimal previousValue, CancellationToken cancellationToken = default)
		{
			int length;
			byte[] userData;
			int read;
			
			length=reader.CurrentFieldHeader.FieldLength;
			if(0<length)
			{
				//Get or create user data attached to this object
				userData=(byte[])reader.GetOrCreateUserData(() => new byte[17]);
				read=userData[16];//In 16th byte store bytes read

				if(length<=16-read)
				{
					await reader.ReadBytesArrayAsync(byteArray: new Memory<byte>(userData, 0, 16-read), cancellationToken: cancellationToken)
						.ConfigureAwait(false);
					if(length==16-read)//Whole array read
					{
						previousValue=ReadDecimal(bytes: userData);
						userData[16]=0;//Clear bytes read - for next decimal in eventually collection
					}
					else//Not whole array read, store current bytes read to 16th byte
						userData[16]=(byte)(read+length);
				}
				else
					throw new SerializationException($"Decimal bytes array to long. Should be 16 bytes. Field no: {reader.CurrentFieldHeader.FieldNo}");
			}

			return previousValue;
		}

		/// <summary>
		/// Is <paramref name="value"/> default value
		/// </summary>
		/// <param name="value">Value to check</param>
		/// <returns>True if <paramref name="value"/> is default value, false otherwise</returns>
		public override bool IsDefault(decimal value)
		{
			return value==default;
		}

		protected decimal ReadDecimal(byte[] bytes)
		{
			Span<int> ints = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, int>(bytes);
			return new decimal(ints[0], ints[1], ints[2], bytes[15]!=0, bytes[14]);
		}
	}
}