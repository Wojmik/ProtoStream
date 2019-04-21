using ProtoStream;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoStreamUnitTestCore.Model;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProtoStreamUnitTestCore
{
	[TestClass]
	public class SerializerListListUnitTest
	{
		[DataTestMethod]
		[DynamicData(nameof(GetSample), DynamicDataSourceType.Method)]
		public async Task SerializeUnitTest(int writerBufferSize, int readerBufferSize, Data1 sample)
		{
			var serializer = new Serializer<Data1>();
			Data1 actual;

			using(var ms = new System.IO.MemoryStream())
			{
				using(var writer = new ProtoStreamWriterTestable(stream: ms, bufferSize: writerBufferSize, leaveOpen: true))
				{
					await serializer.SerializeAsync(writer: writer, value: sample);
				}

				ms.Position=0;

				using(var reader = new ProtoStreamReaderTestable(stream: ms, bufferSize: readerBufferSize))
				{
					actual=await serializer.DeserializeAsync(reader: reader);
				}
			}

			Data1.AreEqual(expected: sample, actual: actual);
		}


		public static IEnumerable<object[]> GetSample()
		{
			yield return CreateSample1().ToObjectArray();
			//yield return CreateSample2().ToObjectArray();
			//yield return CreateSample3().ToObjectArray();
			//yield return CreateSample4().ToObjectArray();
			//yield return CreateSample5().ToObjectArray();
			//yield return CreateSample6().ToObjectArray();
			//yield return CreateSample7().ToObjectArray();
			//yield return CreateSample8().ToObjectArray();
			//yield return CreateSample9().ToObjectArray();
		}

		static SampleData<Data1> CreateSample1()
		{
			const int Nest1Size = 10, Nest2Size = 20;
			SampleData<Data1> sampleData = new SampleData<Data1>() { WriterBufferSize=21, ReaderBufferSize=10, };

			sampleData.SampleObject = new Data1() { Int1=300, Strings111=new List<List<string>>(Nest1Size), };

			for(int i=0; i<Nest1Size; i++)
			{
				sampleData.SampleObject.Strings111.Add(new List<string>(Nest2Size));
				for(int j=0; j<Nest2Size; j++)
					sampleData.SampleObject.Strings111[i].Add($"{i}*{j}");
			}

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample2()
		{
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>() { WriterBufferSize=15, ReaderBufferSize=10, };

			sampleData.SampleObject = new SampleClass() { Int1=int.MinValue, String1="Zażółć gęślą jaźń", };

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample3()
		{
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>() { WriterBufferSize=15, ReaderBufferSize=10, };

			sampleData.SampleObject = new SampleClass() { Int1=int.MaxValue, String1=string.Empty, };

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample4()
		{
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>() { WriterBufferSize=15, ReaderBufferSize=10, };

			sampleData.SampleObject = new SampleClass() { Int1=-800, String1=null, };

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample5()
		{
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>() { WriterBufferSize=18, ReaderBufferSize=13, };

			sampleData.SampleObject = new SampleClass() { Int1=-800, String1="Zażółć gęślą jaźń", Nested1=new SampleNestedClass() { Int11=-500, String11="zAŻÓŁĆ GĘŚLĄ JAŹŃ" } };

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample6()
		{
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>() { WriterBufferSize=18, ReaderBufferSize=13, };

			sampleData.SampleObject = new SampleClass() { Int1=-800, String1="Zażółć gęślą jaźń", Nested1=new SampleNestedClass() { Int11=-500, String11="zAŻÓŁĆ GĘŚLĄ JAŹŃ", ListInt=new List<int>() { 1, 5, -10, 250, }, }, };

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample7()
		{
			const int arraySize = 50;
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>() { WriterBufferSize=18, ReaderBufferSize=13, };
			int i, sign=1;

			sampleData.SampleObject = new SampleClass() { Int1=-800, String1="Zażółć gęślą jaźń", Nested1=new SampleNestedClass() { Int11=-500, String11="zAŻÓŁĆ GĘŚLĄ JAŹŃ", ListInt=new List<int>(arraySize), BytesArray=new byte[arraySize], }, };

			for(i=0; i<arraySize; i++)
			{
				sign=-sign;
				sampleData.SampleObject.Nested1.BytesArray[i]=(byte)(byte.MaxValue-i);
				sampleData.SampleObject.Nested1.ListInt.Add(i*i*sign);//0, 1, -4, 9, -16, ...
			}

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample8()
		{
			const int nestedObjectListSize = 5;
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>() { WriterBufferSize=27, ReaderBufferSize=22, };
			int j;

			sampleData.SampleObject = new SampleClass() { Int1=-800, Nested1=new SampleNestedClass() { Int11=-500,  NestedItems=new List<SampleNestedItemClass>(nestedObjectListSize) }, };

			for(j=0; j<nestedObjectListSize; j++)
			{
				sampleData.SampleObject.Nested1.NestedItems.Add(new SampleNestedItemClass()
				{
					Int111=50*j,
				});
			}

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample9()
		{
			const int arraySize = 50;
			const int nestedObjectListSize = 20;
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>() { WriterBufferSize=27, ReaderBufferSize=22, };
			int i, j, sign=1;

			sampleData.SampleObject = new SampleClass() { Int1=-800, String1="Zażółć gęślą jaźń", Nested1=new SampleNestedClass() { Int11=-500, String11="zAŻÓŁĆ GĘŚLĄ JAŹŃ", ListInt=new List<int>(arraySize), BytesArray=new byte[arraySize], NestedItems=new List<SampleNestedItemClass>(nestedObjectListSize) }, };

			for(i=0; i<arraySize; i++)
			{
				sign=-sign;
				sampleData.SampleObject.Nested1.BytesArray[i]=(byte)(byte.MaxValue-i);
				sampleData.SampleObject.Nested1.ListInt.Add(i*i*sign);//0, 1, -4, 9, -16, ...
			}

			for(j=0; j<nestedObjectListSize; j++)
			{
				sampleData.SampleObject.Nested1.NestedItems.Add(new SampleNestedItemClass()
				{
					Int111=i*j,
					String111=$"{i}*{j}",
					BytesArray=new byte[arraySize],
					ListInt=new List<int>(arraySize),
				});
				for(i=0; i<arraySize; i++)
				{
					sign=-sign;
					sampleData.SampleObject.Nested1.NestedItems[j].BytesArray[i]=(byte)(byte.MaxValue-i-j);
					sampleData.SampleObject.Nested1.NestedItems[j].ListInt.Add(i*j*sign);//0, 1, -4, 9, -16, ...
				}
			}

			return sampleData;
		}
		
		class SampleData<T>
		{
			public int WriterBufferSize { get; set; }
			public int ReaderBufferSize { get; set; }
			public T SampleObject { get; set; }

			public object[] ToObjectArray()
			{
				return new object[]
				{
					this.WriterBufferSize,
					this.ReaderBufferSize,
					this.SampleObject,
				};
			}
		}

		[DataContract]
		public class Data1
		{
			[DataMember(Order = 5)]
			public int Int1 { get; set; }

			[DataMember(Order = 9)]
			public List<List<string>> Strings111 { get; set; }


			public static void AreEqual(Data1 expected, Data1 actual)
			{
				Assert.AreEqual(expected.Int1, actual.Int1);
				//Assert.AreEqual(expected.Long1, actual.Long1);
				//Assert.AreEqual(expected.String1, actual.String1);
				//Assert.AreEqual(expected.ByteArray1, actual.ByteArray1);

				if(expected.Strings111!=null && actual.Strings111!=null)
					if(expected.Strings111.Count==actual.Strings111.Count)
						for(int i = 0; i<expected.Strings111.Count; i++)
						{
							if(expected.Strings111[i]!=null && actual.Strings111[i]!=null)
								if(expected.Strings111[i].Count==actual.Strings111[i].Count)
									for(int j = 0; j<expected.Strings111[i].Count; j++)
										Assert.AreEqual(expected: expected.Strings111[i][j], actual: actual.Strings111[i][j]);
								else
									Assert.Fail("Diferent list sizes");
							else
								Assert.AreEqual(expected.Strings111[i]!=null, actual.Strings111[i]!=null);
						}
					else
						Assert.Fail("Diferent list sizes");
				else
					Assert.AreEqual(expected.Strings111!=null, actual.Strings111!=null);
			}
		}
	}
}