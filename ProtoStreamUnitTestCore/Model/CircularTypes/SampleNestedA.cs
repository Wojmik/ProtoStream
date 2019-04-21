using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ProtoStreamUnitTestCore.Model.CircularTypes
{
	[DataContract]
	class SampleNestedA
	{
		[DataMember(Order = 1)]
		public int IntA1 { get; set; }

		[DataMember(Order = 2)]
		public SampleNestedB NestedB { get; set; }

		[DataMember(Order = 3)]
		public int IntA3 { get; set; }
	}
}