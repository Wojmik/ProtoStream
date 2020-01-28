using System;
using System.Collections.Generic;
using System.Text;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter.InternalModel
{
	delegate bool WriteVarIntHandler<TValue>(Span<byte> destination, TValue value, out int written);
}