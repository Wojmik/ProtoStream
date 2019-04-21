using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for ulong? fields serialized as Fixed
	/// </summary>
	public class FixedULongNullableSerializer : NullableSerializer<ulong>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static FixedULongNullableSerializer Default { get; } = new FixedULongNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public FixedULongNullableSerializer()
			: base(innerSerializer: FixedULongSerializer.Default)
		{ }
	}
}