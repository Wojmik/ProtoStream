using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ProtoStreamUnitTestCore.Model.CircularTypes
{
	[DataContract]
	class SampleNestedB
	{
		[DataMember(Order = 1)]
		public int IntB1 { get; set; }

		[DataMember(Order = 2)]
		public SampleNestedA NestedA { get; set; }

		[DataMember(Order = 3)]
		public int IntB3 { get; set; }
	}
}