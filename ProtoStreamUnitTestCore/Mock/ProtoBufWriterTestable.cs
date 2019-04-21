using ProtoStream;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStreamUnitTestCore
{
	class ProtoStreamWriterTestable : ProtoStreamWriter
	{
		private byte[] OriginBuffer;

		public ProtoStreamWriterTestable(System.IO.Stream stream, int bufferSize = DefaultBufferSize, bool leaveOpen = false)
			 : base(stream: stream, bufferSize: MinBufferSize, leaveOpen: leaveOpen)
		{
			OriginBuffer=this.Buffer;
			this.Buffer=new byte[bufferSize];
			this.NestDataStack.CurrentNestData.BufferAvailableLength=Buffer.Length;
		}

		public (byte[] buffer, int bufferPos) GetInternalBufferData()
		{
			return (buffer: this.Buffer, bufferPos: this.BufferPos);
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				// TODO: wyczyść stan zarządzany (obiekty zarządzane).
				try
				{
					FlushAsync().AsTask().Wait();
				}
				catch
				{ }
			}

			this.Buffer=this.OriginBuffer;

			base.Dispose(disposing);
		}
	}
}