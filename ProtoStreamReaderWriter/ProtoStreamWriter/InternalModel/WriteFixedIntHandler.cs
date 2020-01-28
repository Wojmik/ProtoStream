using System;
using System.Collections.Generic;
using System.Text;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter.InternalModel
{
	delegate bool WriteFixedIntHandler<TValue>(Span<byte> destination, TValue value);
}