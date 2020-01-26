using System;
using System.Collections.Generic;
using System.Text;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter.ProtoStreamWriterInternalModel
{
	/// <summary>
	/// Nested object data
	/// </summary>
	readonly struct NestData
	{
		/// <summary>
		/// Field header - field number with wire type
		/// </summary>
		public readonly ulong FieldHeader;

		/// <summary>
		/// Lenght of the space left for the object size
		/// </summary>
		public readonly int SizeSpaceLength;

		/// <summary>
		/// Position in buffer where object's data is begining
		/// </summary>
		public readonly int DataStartIndex;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="fieldHeader">Field header - field number with wire type</param>
		/// <param name="sizeSpaceLength">Lenght of the space left for the object size</param>
		/// <param name="dataStartIndex">Position in buffer where object's data is begining</param>
		public NestData(ulong fieldHeader, int sizeSpaceLength, int dataStartIndex)
		{
			this.FieldHeader=fieldHeader;
			this.SizeSpaceLength=sizeSpaceLength;
			this.DataStartIndex=dataStartIndex;
		}
	}
}