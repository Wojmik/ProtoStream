using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for int? fields serialized as Fixed
	/// </summary>
	public class FixedIntNullableSerializer : NullableSerializer<int>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static FixedIntNullableSerializer Default { get; } = new FixedIntNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public FixedIntNullableSerializer()
			: base(innerSerializer: FixedIntSerializer.Default)
		{ }
	}
}