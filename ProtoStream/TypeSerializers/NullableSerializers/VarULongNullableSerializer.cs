using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for ulong? fields serialized as VarInt
	/// </summary>
	public class VarULongNullableSerializer : NullableSerializer<ulong>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static VarULongNullableSerializer Default { get; } = new VarULongNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public VarULongNullableSerializer()
			: base(innerSerializer: VarULongSerializer.Default)
		{ }
	}
}