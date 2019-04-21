using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoStream;
using ProtoStreamUnitTestCore.Model.CircularTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStreamUnitTestCore
{
	[TestClass]
	public class NestedTypesSerializatorUnitTest
	{
		[TestMethod]
		public void CircularNestedUnitTest()
		{
			Serializer<SampleNested> serializer;

			serializer=new Serializer<SampleNested>();
		}

		[TestMethod]
		public void CircularNestedABUnitTest()
		{
			Serializer<SampleNestedA> serializer;

			serializer=new Serializer<SampleNestedA>();
		}
	}
}