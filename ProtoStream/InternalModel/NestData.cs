using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.InternalModel
{
	/// <summary>
	/// Nest level data
	/// </summary>
	public class NestData
	{
		/// <summary>
		/// Nest level
		/// </summary>
		public readonly int NestLevel;

		/// <summary>
		/// Field unique number in parent object
		/// </summary>
		public int FieldNo;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="nestLevel">Nest level</param>
		public NestData(int nestLevel)
		{
			this.NestLevel=nestLevel;
		}
	}
}