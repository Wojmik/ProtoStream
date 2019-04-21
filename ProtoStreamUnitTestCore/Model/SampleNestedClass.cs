using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ProtoStreamUnitTestCore.Model
{
	[DataContract]
	public class SampleNestedClass
	{
		[DataMember(Order = 11)]
		public int Int11 { get; set; }

		//[DataMember(Order = 12)]
		//public long Long11 { get; set; }

		[DataMember(Order = 13)]
		public string String11 { get; set; }

		//[DataMember(Order = 14)]
		//public byte[] ByteArray11 { get; set; }

		[DataMember(Order = 19)]
		public List<SampleNestedItemClass> NestedItems { get; set; }

		[DataMember(Order = 20)]
		public List<int> ListInt { get; set; }

		[DataMember(Order = 50)]
		public byte[] BytesArray { get; set; }

		public static void AreEqual(SampleNestedClass expected, SampleNestedClass actual)
		{
			Assert.AreEqual(expected.Int11, actual.Int11);
			//Assert.AreEqual(expected.Long11, actual.Long11);
			Assert.AreEqual(expected.String11, actual.String11);
			//Assert.AreEqual(expected.ByteArray11, actual.ByteArray11);

			if(expected.NestedItems!=null && actual.NestedItems!=null)
				if(expected.NestedItems.Count==actual.NestedItems.Count)
					for(int i = 0; i<expected.NestedItems.Count; i++)
						SampleNestedItemClass.AreEqual(expected: expected.NestedItems[i], actual: actual.NestedItems[i]);
				else
					Assert.Fail("Diferent list sizes");
			else
				Assert.AreEqual(expected.NestedItems!=null, actual.NestedItems!=null);

			if(expected.ListInt!=null && actual.ListInt!=null)
				Assert.IsTrue(Enumerable.SequenceEqual(expected.ListInt, actual.ListInt));
			else
				Assert.AreEqual(expected.ListInt!=null, actual.ListInt!=null);

			if(expected.BytesArray!=null && actual.BytesArray!=null)
				Assert.IsTrue(Enumerable.SequenceEqual(expected.BytesArray, actual.BytesArray));
			else
				Assert.AreEqual(expected.BytesArray!=null, actual.BytesArray!=null);
		}
	}
}