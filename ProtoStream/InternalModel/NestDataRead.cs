using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.InternalModel
{
	/// <summary>
	/// Nest level read data
	/// </summary>
	public class NestDataRead : NestData
	{
		/// <summary>
		/// Data length in this nest level
		/// </summary>
		public int NestLevelLength;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="nestLevel">Nest level</param>
		public NestDataRead(int nestLevel)
			: base(nestLevel: nestLevel)
		{ }

		/// <summary>
		/// Resets object to initial state. Class is preparet to be reusable.
		/// </summary>
		public void Reset(int fieldNo, int nestLevelLength)
		{
			FieldNo=fieldNo;
			NestLevelLength=nestLevelLength;
		}
	}
}