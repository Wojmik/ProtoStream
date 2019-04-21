using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStreamFormatter
{
	class SerializerData
	{
		public ProtoStream.Serializer Serializer { get; set; }

		public SerializeAsyncHandler SerializeAsyncMethod { get; set; }
	}
}