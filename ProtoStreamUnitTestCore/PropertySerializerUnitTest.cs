using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoStream;
using ProtoStream.Internal;
using ProtoStream.PropertySerializers;
using ProtoStreamUnitTestCore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using WojciechMikołajewicz;

namespace ProtoStreamUnitTestCore
{
	[TestClass]
	public class PropertySerializerUnitTest
	{
		[DataTestMethod]
		[DynamicData(nameof(GetSample), DynamicDataSourceType.Method)]
		public async Task PropertySerializerSequenceUnitTest(int bufferSize, List<PropertySerializer<SampleClass>> propertySerializers, SampleClass sampleObject, byte[] bytesExpected)
		{
			using(var ms = new System.IO.MemoryStream())
			{
				using(var writer = new ProtoStreamWriterTestable(stream: ms, bufferSize: bufferSize, leaveOpen: true))
				{
					foreach(var propertySerializer in propertySerializers)
						await propertySerializer.SerializePropertyAsync(objectInstance: sampleObject, writer: writer);
				}

				Assert.IsTrue(Enumerable.SequenceEqual(bytesExpected, ms.ToArray()));
			}
		}

		public static IEnumerable<object[]> GetSample()
		{
			yield return CreateSample1().ToObjectArray();
			yield return CreateSample2().ToObjectArray();
			yield return CreateSample3().ToObjectArray();
		}

		static SampleData<SampleClass> CreateSample1()
		{
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>();

			sampleData.SampleObject = new SampleClass() { Int1=300, String1="To jest test", };
			sampleData.BufferSize=15;
			sampleData.PropertySerializers=new List<PropertySerializer<SampleClass>>()
			{
				new PropertySerializer<SampleClass, int>(typeof(SampleClass).GetProperty(nameof(SampleClass.Int1)), new PropertySerializerSettings(1, SerializationType.Default, false), null),
				new PropertySerializer<SampleClass, string>(typeof(SampleClass).GetProperty(nameof(SampleClass.String1)), new PropertySerializerSettings(3, SerializationType.Default, false), null),
			};
			sampleData.BytesExpected=new byte[21];
			//Int1
			Base128.TryWriteUInt64(destination: sampleData.BytesExpected.AsSpan(0), value: ((uint)sampleData.PropertySerializers[0].FieldNo<<3)|(uint)WireType.VarInt, written: out _);
			Base128.TryWriteInt64(destination: sampleData.BytesExpected.AsSpan(1), value: sampleData.SampleObject.Int1, written: out _);
			//String1 - first 10 bytes
			Base128.TryWriteUInt64(destination: sampleData.BytesExpected.AsSpan(3), value: ((uint)sampleData.PropertySerializers[1].FieldNo<<3)|(uint)WireType.LengthDelimited, written: out _);
			Base128.TryWriteUInt64(destination: sampleData.BytesExpected.AsSpan(4), value: 9, written: out _);
			Encoding.UTF8.GetBytes(sampleData.SampleObject.String1, 0, 9, sampleData.BytesExpected, 6);
			//String1 - next 2 bytes
			Base128.TryWriteUInt64(destination: sampleData.BytesExpected.AsSpan(15), value: ((uint)sampleData.PropertySerializers[1].FieldNo<<3)|(uint)WireType.LengthDelimited, written: out _);
			Base128.TryWriteUInt64(destination: sampleData.BytesExpected.AsSpan(16), value: 3, written: out _);
			Encoding.UTF8.GetBytes(sampleData.SampleObject.String1, 9, sampleData.SampleObject.String1.Length-9, sampleData.BytesExpected, 18);

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample2()
		{
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>();

			sampleData.SampleObject = new SampleClass() { Int1=300, String1="To jest test", };
			sampleData.BufferSize=15;
			sampleData.PropertySerializers=new List<PropertySerializer<SampleClass>>()
			{
				new PropertySerializer<SampleClass, string>(typeof(SampleClass).GetProperty(nameof(SampleClass.String1)), new PropertySerializerSettings(3, SerializationType.Default, false), null),
				new PropertySerializer<SampleClass, int>(typeof(SampleClass).GetProperty(nameof(SampleClass.Int1)), new PropertySerializerSettings(1, SerializationType.Default, false), null),
			};
			sampleData.BytesExpected=new byte[18];
			//String1 - first 12 bytes
			Base128.TryWriteUInt64(destination: sampleData.BytesExpected.AsSpan(0), value: ((uint)sampleData.PropertySerializers[0].FieldNo<<3)|(uint)WireType.LengthDelimited, written: out _);
			Base128.TryWriteUInt64(destination: sampleData.BytesExpected.AsSpan(1), value: 12, written: out _);
			Encoding.UTF8.GetBytes(sampleData.SampleObject.String1, 0, 12, sampleData.BytesExpected, 3);
			//Int1
			Base128.TryWriteUInt64(destination: sampleData.BytesExpected.AsSpan(15), value: ((uint)sampleData.PropertySerializers[1].FieldNo<<3)|(uint)WireType.VarInt, written: out _);
			Base128.TryWriteInt64(destination: sampleData.BytesExpected.AsSpan(16), value: sampleData.SampleObject.Int1, written: out _);

			return sampleData;
		}

		static SampleData<SampleClass> CreateSample3()
		{
			SampleData<SampleClass> sampleData = new SampleData<SampleClass>();

			sampleData.SampleObject = new SampleClass() { Int1=300, String1="Zażółć gęślą jaźń", };
			sampleData.BufferSize=15;
			sampleData.PropertySerializers=new List<PropertySerializer<SampleClass>>()
			{
				new PropertySerializer<SampleClass, string>(typeof(SampleClass).GetProperty(nameof(SampleClass.String1)), new PropertySerializerSettings(3, SerializationType.Default, false), null),
				new PropertySerializer<SampleClass, int>(typeof(SampleClass).GetProperty(nameof(SampleClass.Int1)), new PropertySerializerSettings(1, SerializationType.Default, false), null),
			};
			sampleData.BytesExpected=new byte[38];
			//String1 - first 12 bytes
			Base128.TryWriteUInt64(destination: sampleData.BytesExpected.AsSpan(0), value: ((uint)sampleData.PropertySerializers[0].FieldNo<<3)|(uint)WireType.LengthDelimited, written: out _);
			Base128.TryWriteUInt64(destination: sampleData.BytesExpected.AsSpan(1), value: 12, written: out _);
			Encoding.UTF8.GetBytes(sampleData.SampleObject.String1, 0, 8, sampleData.BytesExpected, 3);
			//String1 - next 12 bytes
			Base128.TryWriteUInt64(destination: sampleData.BytesExpected.AsSpan(15), value: ((uint)sampleData.PropertySerializers[0].FieldNo<<3)|(uint)WireType.LengthDelimited, written: out _);
			Base128.TryWriteUInt64(destination: sampleData.BytesExpected.AsSpan(16), value: 12, written: out _);
			Encoding.UTF8.GetBytes(sampleData.SampleObject.String1, 8, 8, sampleData.BytesExpected, 18);
			//String1 - next 2 bytes
			Base128.TryWriteUInt64(destination: sampleData.BytesExpected.AsSpan(30), value: ((uint)sampleData.PropertySerializers[0].FieldNo<<3)|(uint)WireType.LengthDelimited, written: out _);
			Base128.TryWriteUInt64(destination: sampleData.BytesExpected.AsSpan(31), value: 2, written: out _);
			Encoding.UTF8.GetBytes(sampleData.SampleObject.String1, 16, sampleData.SampleObject.String1.Length-16, sampleData.BytesExpected, 33);
			//Int1
			Base128.TryWriteUInt64(destination: sampleData.BytesExpected.AsSpan(35), value: ((uint)sampleData.PropertySerializers[1].FieldNo<<3)|(uint)WireType.VarInt, written: out _);
			Base128.TryWriteInt64(destination: sampleData.BytesExpected.AsSpan(36), value: sampleData.SampleObject.Int1, written: out _);

			return sampleData;
		}

		class SampleData<T>
		{
			public int BufferSize { get; set; }
			public List<PropertySerializer<T>> PropertySerializers { get; set; }
			public SampleClass SampleObject { get; set; }
			public byte[] BytesExpected { get; set; }

			public object[] ToObjectArray()
			{
				return new object[]
				{
					this.BufferSize,
					this.PropertySerializers,
					this.SampleObject,
					this.BytesExpected,
				};
			}
		}
	}
}