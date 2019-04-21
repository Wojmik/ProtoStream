using ProtoStream;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStreamUnitTestCore
{
	class ProtoStreamReaderTestable : ProtoStreamReader
	{
		private byte[] OriginBuffer;

		public ProtoStreamReaderTestable(System.IO.Stream stream, int bufferSize = DefaultBufferSize, bool leaveOpen = false)
			 : base(stream: stream, bufferSize: MinBufferSize, leaveOpen: leaveOpen)
		{
			OriginBuffer=this.Buffer;
			this.Buffer=new byte[bufferSize];
		}

		public (byte[] buffer, int bufferPos, int bufferLength) GetInternalBufferData()
		{
			return (buffer: this.Buffer, bufferPos: this.BufferPos, bufferLength: this.BufferLength);
		}

		protected override void Dispose(bool disposing)
		{
			this.Buffer=this.OriginBuffer;

			base.Dispose(disposing);
		}
	}
}