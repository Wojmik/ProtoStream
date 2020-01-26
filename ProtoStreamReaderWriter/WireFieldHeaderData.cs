using System;
using System.Collections.Generic;
using System.Text;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter
{
	/// <summary>
	/// Wire field header data
	/// </summary>
	public readonly struct WireFieldHeaderData
	{
		/// <summary>
		/// Wire type
		/// </summary>
		public readonly WireType WireType;

		/// <summary>
		/// Field number
		/// </summary>
		public readonly int FieldNo;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="wireType">Wire type</param>
		/// <param name="fieldNo">Field number</param>
		public WireFieldHeaderData(WireType wireType, int fieldNo)
		{
			this.WireType=wireType;
			this.FieldNo=fieldNo;
		}

		/// <summary>
		/// Decompose
		/// </summary>
		/// <param name="wireType">Wire type</param>
		/// <param name="fieldNo">Field number</param>
		public void Decompose(out WireType wireType, out int fieldNo)
		{
			wireType=this.WireType;
			fieldNo=this.FieldNo;
		}
	}
}