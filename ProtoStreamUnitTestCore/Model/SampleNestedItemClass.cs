using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ProtoStreamUnitTestCore.Model
{
	[DataContract]
	public class SampleNestedItemClass
	{
		[DataMember(Order = 101)]
		public int Int111 { get; set; }

		//[DataMember(Order = 102)]
		//public long Long111 { get; set; }

		[DataMember(Order = 103)]
		public string String111 { get; set; }

		//[DataMember(Order = 104)]
		//public byte[] ByteArray111 { get; set; }

		[DataMember(Order = 110)]
		public List<int> ListInt { get; set; }

		[DataMember(Order = 150)]
		public byte[] BytesArray { get; set; }

		public static void AreEqual(SampleNestedItemClass expected, SampleNestedItemClass actual)
		{
			Assert.AreEqual(expected.Int111, actual.Int111);
			//Assert.AreEqual(expected.Long111, actual.Long111);
			Assert.AreEqual(expected.String111, actual.String111);
			//Assert.AreEqual(expected.ByteArray111, actual.ByteArray111);

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