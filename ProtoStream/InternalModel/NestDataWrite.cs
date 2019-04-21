using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.InternalModel
{
	/// <summary>
	/// Nest level write data
	/// </summary>
	public class NestDataWrite : NestData
	{
		/// <summary>
		/// Header length (wire type with field no + place left for length)
		/// </summary>
		public int HeaderLength;

		/// <summary>
		/// Index in the buffer where this nest level data starts
		/// </summary>
		public int LevelDataStartIndex;

		/// <summary>
		/// Space in buffer
		/// </summary>
		public int BufferAvailableLength;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="nestLevel">Nest level</param>
		public NestDataWrite(int nestLevel)
			: base(nestLevel: nestLevel)
		{ }

		/// <summary>
		/// Resets object to initial state. Class is preparet to be reusable.
		/// </summary>
		/// <param name="fieldNo">Field unique number in parent object</param>
		/// <param name="headerLength">Header length (wire type with field no + place left for length)</param>
		/// <param name="levelDataStartIndex">Index in the bufer where data of this nest level starts</param>
		/// <param name="bufferAvailableLength">Space in buffer</param>
		public void Reset(int fieldNo, int headerLength, int levelDataStartIndex, int bufferAvailableLength)
		{
			FieldNo=fieldNo;
			HeaderLength=headerLength;
			LevelDataStartIndex=levelDataStartIndex;
			BufferAvailableLength=bufferAvailableLength;
		}
	}
}