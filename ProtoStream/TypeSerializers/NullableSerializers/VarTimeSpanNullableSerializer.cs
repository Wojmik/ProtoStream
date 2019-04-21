using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for TimeSpan? fields serialized as VarInt
	/// </summary>
	public class VarTimeSpanNullableSerializer : NullableSerializer<TimeSpan>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static VarTimeSpanNullableSerializer Default { get; } = new VarTimeSpanNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public VarTimeSpanNullableSerializer()
			: base(innerSerializer: VarTimeSpanSerializer.Default)
		{ }
	}
}