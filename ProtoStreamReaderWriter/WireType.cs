using System;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter
{
	/// <summary>
	/// Wire type
	/// </summary>
	public enum WireType
	{
		/// <summary>
		/// Variable lenght integer number
		/// </summary>
		VarInt = 0,

		/// <summary>
		/// 64-bit number
		/// </summary>
		Fixed64 = 1,

		/// <summary>
		/// Length delimited field
		/// </summary>
		LengthDelimited = 2,

		/// <summary>
		/// 32-bit number
		/// </summary>
		Fixed32 = 5,
	}
}