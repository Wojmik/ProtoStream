using System;
using System.Collections.Generic;
using System.Text;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter.ProtoStreamReaderInternalModel
{
	/// <summary>
	/// Data for nesting object
	/// </summary>
	readonly struct NestData
	{
		/// <summary>
		/// End position of nested object
		/// </summary>
		public readonly ulong EndObjectPosition;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="endObjectPosition">End position of nested object</param>
		public NestData(ulong endObjectPosition)
		{
			this.EndObjectPosition=endObjectPosition;
		}
	}
}