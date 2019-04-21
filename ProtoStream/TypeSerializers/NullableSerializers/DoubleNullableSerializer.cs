using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for double? fields
	/// </summary>
	public class DoubleNullableSerializer : NullableSerializer<double>
	{
		/// <summary>
		/// Serializer for nullable double fields
		/// </summary>
		public static DoubleNullableSerializer Default { get; } = new DoubleNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public DoubleNullableSerializer()
			: base(innerSerializer: DoubleSerializer.Default)
		{ }
	}
}