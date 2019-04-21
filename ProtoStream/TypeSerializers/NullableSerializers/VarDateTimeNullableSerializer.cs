using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for DateTime? fields serialized as VarInt
	/// </summary>
	public class VarDateTimeNullableSerializer : NullableSerializer<DateTime>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static VarDateTimeNullableSerializer Default { get; } = new VarDateTimeNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public VarDateTimeNullableSerializer()
			: base(innerSerializer: VarDateTimeSerializer.Default)
		{ }
	}
}