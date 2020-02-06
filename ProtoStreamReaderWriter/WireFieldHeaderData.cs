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
		/// Field number
		/// </summary>
		public readonly int FieldNo;

		/// <summary>
		/// Wire type
		/// </summary>
		public readonly WireType WireType;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="fieldNo">Field number</param>
		/// <param name="wireType">Wire type</param>
		public WireFieldHeaderData(int fieldNo, WireType wireType)
		{
			this.FieldNo=fieldNo;
			this.WireType=wireType;
		}

		/// <summary>
		/// Deconstruct
		/// </summary>
		/// <param name="fieldNo">Field number</param>
		/// <param name="wireType">Wire type</param>
		public void Deconstruct(out int fieldNo, out WireType wireType)
		{
			fieldNo=this.FieldNo;
			wireType=this.WireType;
		}
	}
}