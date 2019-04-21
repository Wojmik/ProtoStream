using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for bool? fields serialized as VarInt
	/// </summary>
	public class BoolNullableSerializer : NullableSerializer<bool>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static BoolNullableSerializer Default { get; } = new BoolNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public BoolNullableSerializer()
			: base(innerSerializer: BoolSerializer.Default)
		{ }
	}
}