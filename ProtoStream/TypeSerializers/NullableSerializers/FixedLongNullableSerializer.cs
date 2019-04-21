using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for long? fields serialized as Fixed
	/// </summary>
	public class FixedLongNullableSerializer : NullableSerializer<long>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static FixedLongNullableSerializer Default { get; } = new FixedLongNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public FixedLongNullableSerializer()
			: base(innerSerializer: FixedLongSerializer.Default)
		{ }
	}
}