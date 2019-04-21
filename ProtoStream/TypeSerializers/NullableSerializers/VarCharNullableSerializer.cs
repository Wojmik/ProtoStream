using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for char? fields serialized as VarInt
	/// </summary>
	public class VarCharNullableSerializer : NullableSerializer<char>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static VarCharNullableSerializer Default { get; } = new VarCharNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public VarCharNullableSerializer()
			: base(innerSerializer: VarCharSerializer.Default)
		{ }
	}
}