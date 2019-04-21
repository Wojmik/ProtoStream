using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.Model
{
	/// <summary>
	/// Event saved in field no
	/// </summary>
	public enum FieldNoEvent : int
	{
		/// <summary>
		/// Start group
		/// </summary>
		StartGroup = -1,

		/// <summary>
		/// End group
		/// </summary>
		EndGroup = -2,

		/// <summary>
		/// Leaving nested object
		/// </summary>
		LeavingNestedObject = -5,

		/// <summary>
		/// End of stream
		/// </summary>
		EndOfStream = -10,
	}
}