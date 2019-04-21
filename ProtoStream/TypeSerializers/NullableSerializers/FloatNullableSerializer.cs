using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for float? fields
	/// </summary>
	public class FloatNullableSerializer : NullableSerializer<float>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static FloatNullableSerializer Default { get; } = new FloatNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public FloatNullableSerializer()
			: base(innerSerializer: FloatSerializer.Default)
		{ }
	}
}