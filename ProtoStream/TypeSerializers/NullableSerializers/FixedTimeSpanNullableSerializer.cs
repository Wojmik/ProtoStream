using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for TimeSpan? fields serialized as Fixed
	/// </summary>
	public class FixedTimeSpanNullableSerializer : NullableSerializer<TimeSpan>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static FixedTimeSpanNullableSerializer Default { get; } = new FixedTimeSpanNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public FixedTimeSpanNullableSerializer()
			: base(innerSerializer: FixedTimeSpanSerializer.Default)
		{ }
	}
}