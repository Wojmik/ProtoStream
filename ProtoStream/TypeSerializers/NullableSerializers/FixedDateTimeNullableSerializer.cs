using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for DateTime? fields serialized as Fixed
	/// </summary>
	public class FixedDateTimeNullableSerializer : NullableSerializer<DateTime>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static FixedDateTimeNullableSerializer Default { get; } = new FixedDateTimeNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public FixedDateTimeNullableSerializer()
			: base(innerSerializer: FixedDateTimeSerializer.Default)
		{ }
	}
}