using ProtoStream;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoStreamFormatter
{
	delegate ValueTask SerializeAsyncHandler(Serializer serializer, ProtoStreamWriter writer, object value, CancellationToken cancellationToken);
}