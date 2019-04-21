using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for uint? fields serialized as VarInt
	/// </summary>
	public class VarUIntNullableSerializer : NullableSerializer<uint>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static VarUIntNullableSerializer Default { get; } = new VarUIntNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public VarUIntNullableSerializer()
			: base(innerSerializer: VarUIntSerializer.Default)
		{ }
	}
}