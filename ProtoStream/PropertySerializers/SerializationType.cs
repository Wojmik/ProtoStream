using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.PropertySerializers
{
	/// <summary>
	/// Serialization type
	/// </summary>
	public enum SerializationType
	{
		/// <summary>
		/// Default
		/// </summary>
		Default,

		/// <summary>
		/// Variable integer
		/// </summary>
		VarInt,

		/// <summary>
		/// Fixed size
		/// </summary>
		FixedSize,
	}
}