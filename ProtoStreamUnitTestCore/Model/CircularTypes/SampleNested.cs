using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ProtoStreamUnitTestCore.Model.CircularTypes
{
	[DataContract]
	class SampleNested
	{
		[DataMember(Order = 1)]
		public int Int1 { get; set; }

		[DataMember(Order = 2)]
		public SampleNested Nested { get; set; }

		[DataMember(Order = 3)]
		public int Int3 { get; set; }
	}
}