using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for long? fields serialized as VarInt
	/// </summary>
	public class VarLongNullableSerializer : NullableSerializer<long>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static VarLongNullableSerializer Default { get; } = new VarLongNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public VarLongNullableSerializer()
			: base(innerSerializer: VarLongSerializer.Default)
		{ }
	}
}