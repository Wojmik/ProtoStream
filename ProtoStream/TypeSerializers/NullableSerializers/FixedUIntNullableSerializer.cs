using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for uint? fields serialized as Fixed
	/// </summary>
	public class FixedUIntNullableSerializer : NullableSerializer<uint>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static FixedUIntNullableSerializer Default { get; } = new FixedUIntNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public FixedUIntNullableSerializer()
			: base(innerSerializer: FixedUIntSerializer.Default)
		{ }
	}
}