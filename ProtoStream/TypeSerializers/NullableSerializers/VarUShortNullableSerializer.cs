using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for ushort fields serialized as VarInt
	/// </summary>
	public class VarUShortNullableSerializer : NullableSerializer<ushort>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static VarUShortNullableSerializer Default { get; } = new VarUShortNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public VarUShortNullableSerializer()
			: base(innerSerializer: VarUShortSerializer.Default)
		{ }
	}
}