using System;
using System.Collections.Generic;
using System.Text;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter
{
	/// <summary>
	/// Struct contains data of the read field header
	/// </summary>
	public readonly struct ReadFieldHeaderResult
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
		/// Length of variable length field's data in bytes or zero for other wire types
		/// </summary>
		public readonly ulong FieldLength;

		/// <summary>
		/// Is end of nested object
		/// </summary>
		public readonly bool EndOfObject;

		/// <summary>
		/// Is end of stream
		/// </summary>
		public readonly bool EndOfStream;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="fieldNo">Field number</param>
		/// <param name="wireType">Wire type</param>
		/// <param name="fieldLength">Length of variable length field's data in bytes or zero for other wire types</param>
		/// <param name="endOfObject">Is end of nested object</param>
		/// <param name="endOfStream">Is end of stream</param>
		public ReadFieldHeaderResult(int fieldNo, WireType wireType, ulong fieldLength, bool endOfObject, bool endOfStream)
		{
			this.FieldNo=fieldNo;
			this.WireType=wireType;
			this.FieldLength=fieldLength;
			this.EndOfObject=endOfObject;
			this.EndOfStream=endOfStream;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="fieldHeader">Field header</param>
		/// <param name="fieldLength">Length of variable length field's data in bytes or zero for other wire types</param>
		/// <param name="endOfObject">Is end of nested object</param>
		/// <param name="endOfStream">Is end of stream</param>
		public ReadFieldHeaderResult(in WireFieldHeaderData fieldHeader, ulong fieldLength, bool endOfObject, bool endOfStream)
		{
			this.FieldNo=fieldHeader.FieldNo;
			this.WireType=fieldHeader.WireType;
			this.FieldLength=fieldLength;
			this.EndOfObject=endOfObject;
			this.EndOfStream=endOfStream;
		}

		/// <summary>
		/// Deconstruct
		/// </summary>
		/// <param name="fieldNo">Field number</param>
		/// <param name="wireType">Wire type</param>
		/// <param name="fieldLength">Length of variable length field's data in bytes or zero for other wire types</param>
		/// <param name="endOfObject">Is end of nested object</param>
		/// <param name="endOfStream">Is end of stream</param>
		public void Deconstruct(out int fieldNo, out WireType wireType, out ulong fieldLength, out bool endOfObject, out bool endOfStream)
		{
			fieldNo=this.FieldNo;
			wireType=this.WireType;
			fieldLength=this.FieldLength;
			endOfObject=this.EndOfObject;
			endOfStream=this.EndOfStream;
		}
	}
}