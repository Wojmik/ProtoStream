using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for int? fields serialized as VarInt
	/// </summary>
	public class VarIntNullableSerializer : NullableSerializer<int>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static VarIntNullableSerializer Default { get; } = new VarIntNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public VarIntNullableSerializer()
			: base(innerSerializer: VarIntSerializer.Default)
		{ }
	}
}