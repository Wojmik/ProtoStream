using System;
using System.Collections.Generic;
using System.Text;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter.InternalModel
{
	public delegate bool TryWriteSimpleFieldHandler<TValue>(Span<byte> destination, TValue value, out int written);
}