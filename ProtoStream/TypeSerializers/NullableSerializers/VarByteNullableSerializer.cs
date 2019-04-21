using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for byte? fields serialized as VarInt
	/// </summary>
	public class VarByteNullableSerializer : NullableSerializer<byte>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static VarByteNullableSerializer Default { get; } = new VarByteNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public VarByteNullableSerializer()
			: base(innerSerializer: VarByteSerializer.Default)
		{ }
	}
}