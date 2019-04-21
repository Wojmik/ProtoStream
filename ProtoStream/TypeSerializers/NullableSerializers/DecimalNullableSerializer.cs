using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for decimal? fields
	/// </summary>
	public class DecimalNullableSerializer : NullableSerializer<decimal>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static DecimalNullableSerializer Default { get; } = new DecimalNullableSerializer();

		/// <summary>
		/// Constructor
		/// </summary>
		public DecimalNullableSerializer()
			: base(innerSerializer: DecimalSerializer.Default)
		{ }
	}
}