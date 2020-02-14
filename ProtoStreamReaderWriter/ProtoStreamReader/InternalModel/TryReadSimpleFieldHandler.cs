using System;
using System.Collections.Generic;
using System.Text;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter.InternalModel
{
	public delegate bool TryReadSimpleFieldHandler<TValue>(ReadOnlySpan<byte> source, out TValue value, out int read);
}