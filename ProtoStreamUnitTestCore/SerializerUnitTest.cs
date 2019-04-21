using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoStream;
using ProtoStreamUnitTestCore.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProtoStreamUnitTestCore
{
	[TestClass]
	public class SerializerUnitTest
	{
		[DataTestMethod]
		[DynamicData(nameof(GetSample), DynamicDataSourceType.Method)]
		public async Task SerializeUnitTest(int writerBufferSize, int readerBufferSize, SampleClass sample)
		{
			var serializer = new Serializer<SampleClass>();
			SampleClass actual;

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

			SampleClass.AreEqual(expected: sample, actual: actual);
		}


		public static IEnumerable<object[]> GetSample()
		{
			yield return CreateSample1().ToObjectArray();
			yield return CreateSample2().ToObjectArray();
			yield return CreateSample3().ToObjectArray();
			yield return CreateSample4().ToObjectArray();
			yield return CreateSample5().ToObjectArray();
			yield return CreateSample6().ToObjectArray();
			yield return CreateSample7().ToObjectArray();
			yield return CreateSample8().ToObjectArray();
			yield return CreateSample9().ToObjectArray();
			yield return CreateSample10().ToObjectArray();
			yield return CreateSample11().ToObjectArray();
		}

		static SampleData<SampleClass> CreateSample1()
		{
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>() { WriterBufferSize=16, ReaderBufferSize=10, };

			sampleData.SampleObject = new SampleClass() { Int1=300, String1="To jest test", };

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample2()
		{
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>() { WriterBufferSize=16, ReaderBufferSize=10, };

			sampleData.SampleObject = new SampleClass() { Int1=int.MinValue, String1="Zażółć gęślą jaźń", };

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample3()
		{
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>() { WriterBufferSize=16, ReaderBufferSize=10, };

			sampleData.SampleObject = new SampleClass() { Int1=int.MaxValue, String1=string.Empty, };

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample4()
		{
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>() { WriterBufferSize=16, ReaderBufferSize=10, };

			sampleData.SampleObject = new SampleClass() { Int1=-800, String1=null, Float1=float.Epsilon, Double1=double.Epsilon, FloatN1=float.Epsilon, DoubleN1=double.Epsilon, };

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample5()
		{
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>() { WriterBufferSize=20, ReaderBufferSize=10, };

			sampleData.SampleObject = new SampleClass()
			{
				Int1=int.MinValue, UInt1=uint.MinValue, Long1=long.MinValue, ULong1=ulong.MinValue, Bool1=false, Char1=char.MinValue, DateTime1=DateTime.MinValue, TimeSpan1=TimeSpan.MinValue,
				SByte1=sbyte.MinValue, Byte1=byte.MinValue, Short1=short.MinValue, UShort1=ushort.MinValue, DayOfWeek1=DayOfWeek.Sunday,
				Float1=float.MinValue, Double1=double.MinValue, Decimal1=decimal.MinValue,
				String1="Zażółć gęślą jaźń",
				IntN1=int.MinValue, UIntN1=uint.MinValue, LongN1=long.MinValue, ULongN1=ulong.MinValue, BoolN1=false, CharN1=char.MinValue, DateTimeN1=DateTime.MinValue, TimeSpanN1=TimeSpan.MinValue,
				SByteN1=sbyte.MinValue, ByteN1=byte.MinValue, ShortN1=short.MinValue, UShortN1=ushort.MinValue, DayOfWeekN1=DayOfWeek.Sunday,
				FloatN1=float.MinValue, DoubleN1=double.MinValue, DecimalN1=decimal.MinValue,
			};

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample6()
		{
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>() { WriterBufferSize=20, ReaderBufferSize=10, };

			sampleData.SampleObject = new SampleClass()
			{
				Int1=int.MaxValue, UInt1=uint.MaxValue, Long1=long.MaxValue, ULong1=ulong.MaxValue, Bool1=true, Char1=char.MaxValue, DateTime1=DateTime.MaxValue, TimeSpan1=TimeSpan.MaxValue,
				SByte1=sbyte.MaxValue, Byte1=byte.MaxValue, Short1=short.MaxValue, UShort1=ushort.MaxValue, DayOfWeek1=DayOfWeek.Saturday,
				Float1=float.MaxValue, Double1=double.MaxValue, Decimal1=decimal.MaxValue,
				String1="Zażółć gęślą jaźń",
				IntN1=int.MaxValue, UIntN1=uint.MaxValue, LongN1=long.MaxValue, ULongN1=ulong.MaxValue, BoolN1=true, CharN1=char.MaxValue, DateTimeN1=DateTime.MaxValue, TimeSpanN1=TimeSpan.MaxValue,
				SByteN1=sbyte.MaxValue, ByteN1=byte.MaxValue, ShortN1=short.MaxValue, UShortN1=ushort.MaxValue, DayOfWeekN1=DayOfWeek.Saturday,
				FloatN1=float.MaxValue, DoubleN1=double.MaxValue, DecimalN1=decimal.MaxValue,
			};

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample7()
		{
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>() { WriterBufferSize=18, ReaderBufferSize=13, };

			sampleData.SampleObject = new SampleClass() { Int1=-800, String1="Zażółć gęślą jaźń", Nested1=new SampleNestedClass() { Int11=-500, String11="zAŻÓŁĆ GĘŚLĄ JAŹŃ" } };

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample8()
		{
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>() { WriterBufferSize=18, ReaderBufferSize=13, };

			sampleData.SampleObject = new SampleClass() { Int1=-800, String1="Zażółć gęślą jaźń", Nested1=new SampleNestedClass() { Int11=-500, String11="zAŻÓŁĆ GĘŚLĄ JAŹŃ", ListInt=new List<int>() { 1, 5, -10, 250, }, }, };

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample9()
		{
			const int arraySize = 50;
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>() { WriterBufferSize=18, ReaderBufferSize=13, };
			int i, sign = 1;

			sampleData.SampleObject = new SampleClass() { Int1=-800, String1="Zażółć gęślą jaźń", Nested1=new SampleNestedClass() { Int11=-500, String11="zAŻÓŁĆ GĘŚLĄ JAŹŃ", ListInt=new List<int>(arraySize), BytesArray=new byte[arraySize], }, };

			for(i=0; i<arraySize; i++)
			{
				sign=-sign;
				sampleData.SampleObject.Nested1.BytesArray[i]=(byte)(byte.MaxValue-i);
				sampleData.SampleObject.Nested1.ListInt.Add(i*i*sign);//0, 1, -4, 9, -16, ...
			}

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample10()
		{
			const int nestedObjectListSize = 5;
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>() { WriterBufferSize=27, ReaderBufferSize=22, };
			int j;

			sampleData.SampleObject = new SampleClass() { Int1=-800, Nested1=new SampleNestedClass() { Int11=-500, NestedItems=new List<SampleNestedItemClass>(nestedObjectListSize) }, };

			for(j=0; j<nestedObjectListSize; j++)
			{
				sampleData.SampleObject.Nested1.NestedItems.Add(new SampleNestedItemClass()
				{
					Int111=50*j,
				});
			}

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample11()
		{
			const int arraySize = 50;
			const int nestedObjectListSize = 20;
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>() { WriterBufferSize=27, ReaderBufferSize=22, };
			int i, j, sign = 1;

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
	}
}