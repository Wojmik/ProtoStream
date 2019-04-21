using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for enum fields based on long, serialized as VarInt
	/// </summary>
	public class LongEnumSerializer<TEnum> : EnumSerializerBase<TEnum>
	{
		/// <summary>
		/// Converts enum to long
		/// </summary>
		/// <param name="value">Enum value to convert</param>
		/// <returns>Long value of the enum value</returns>
		protected override long ToLong(TEnum value)
		{
			return (long)(object)value;
		}

		/// <summary>
		/// Converts long to enum
		/// </summary>
		/// <param name="value">Long value to convert</param>
		/// <returns>Enum value of the long value</returns>
		protected override TEnum ToEnum(long value)
		{
			return (TEnum)(object)value;
		}
	}
}