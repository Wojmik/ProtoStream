using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream
{
	/// <summary>
	/// Field header
	/// </summary>
	public struct FieldHeader
	{
		/// <summary>
		/// Wire data type
		/// </summary>
		public Internal.WireType WireType;

		/// <summary>
		/// Field unique number. Minus one for object out, minus two for end of transmission.
		/// </summary>
		public int FieldNo;

		/// <summary>
		/// Field length for length delimited fields
		/// </summary>
		public int FieldLength;

		/// <summary>
		/// Returns string representation of the object
		/// </summary>
		/// <returns>String representation of the object</returns>
		public override string ToString()
		{
			return $"FieldNo: {this.FieldNo}, WireType: {this.WireType}, Length: {this.FieldLength}";
		}
	}
}