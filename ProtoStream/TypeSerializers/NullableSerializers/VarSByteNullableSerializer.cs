using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for sbyte? fields serialized as VarInt
	/// </summary>
	public class VarSByteNullableSerializer : NullableSerializer<sbyte>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static VarSByteNullableSerializer Default { get; } = new VarSByteNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public VarSByteNullableSerializer()
			: base(innerSerializer: VarSByteSerializer.Default)
		{ }
	}
}