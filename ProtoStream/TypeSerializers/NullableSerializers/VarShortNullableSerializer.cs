using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for short? fields serialized as VarInt
	/// </summary>
	public class VarShortNullableSerializer : NullableSerializer<short>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static VarShortNullableSerializer Default { get; } = new VarShortNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public VarShortNullableSerializer()
			: base(innerSerializer: VarShortSerializer.Default)
		{ }
	}
}