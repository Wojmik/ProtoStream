using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter.InternalModel
{
	delegate ValueTask<T> ReadIntegerAsyncHandler<T>(ProtoStreamReader psr, CancellationToken cancellationToken)
		where T : struct;
}