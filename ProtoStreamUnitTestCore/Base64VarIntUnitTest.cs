using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using static ProtoStream.Internal.Base64VarInt;

namespace ProtoStreamUnitTestCore
{
	[TestClass]
	public class Base64VarIntUnitTest
	{
		//[DataTestMethod]
		//[DynamicData(nameof(GetLongTestData), dynamicDataSourceType: DynamicDataSourceType.Method)]
		//public void LongTestMethod(long value, byte[] serialized)
		//{
		//	byte[] buf = new byte[serialized.Length];
		//	long readValue;
		//	int written, read;
		//	bool success;

		//	success=TryWriteInt64VarInt(destination: buf, value: value, written: out written);
		//	Assert.IsTrue(success);
		//	Assert.AreEqual(expected: serialized.Length, actual: written);
		//	Assert.IsTrue(Enumerable.SequenceEqual(serialized, buf));

		//	success=TryReadInt64VarInt(source: serialized, value: out readValue, read: out read);
		//	Assert.IsTrue(success);
		//	Assert.AreEqual(expected: serialized.Length, actual: read);
		//	Assert.AreEqual(expected: value, actual: readValue);
		//}

		//[DataTestMethod]
		//[DynamicData(nameof(GetLongUTestData), dynamicDataSourceType: DynamicDataSourceType.Method)]
		//public void LongUTestMethod(ulong value, byte[] serialized)
		//{
		//	byte[] buf = new byte[serialized.Length];
		//	ulong readValue;
		//	int written, read;
		//	bool success;

		//	success=TryWriteInt64VarInt(destination: buf, value: value, written: out written);
		//	Assert.IsTrue(success);
		//	Assert.AreEqual(expected: serialized.Length, actual: written);
		//	Assert.IsTrue(Enumerable.SequenceEqual(serialized, buf));

		//	success=TryReadInt64VarInt(source: serialized, value: out readValue, read: out read);
		//	Assert.IsTrue(success);
		//	Assert.AreEqual(expected: serialized.Length, actual: read);
		//	Assert.AreEqual(expected: value, actual: readValue);
		//}

		[DataTestMethod]
		[DynamicData(nameof(GetLongZigZagTestData), dynamicDataSourceType: DynamicDataSourceType.Method)]
		public void LongTestMethod(long value, byte[] serialized)
		{
			byte[] buf = new byte[serialized.Length];
			long readValue;
			int written, read;
			bool success;

			success=TryWriteInt64VarInt(destination: buf, value: value, written: out written);
			Assert.IsTrue(success);
			Assert.AreEqual(expected: serialized.Length, actual: written);
			Assert.IsTrue(Enumerable.SequenceEqual(serialized, buf));

			success=TryReadInt64VarInt(source: serialized, value: out readValue, read: out read);
			Assert.IsTrue(success);
			Assert.AreEqual(expected: serialized.Length, actual: read);
			Assert.AreEqual(expected: value, actual: readValue);
		}

		[DataTestMethod]
		[DynamicData(nameof(GetULongTestData), dynamicDataSourceType: DynamicDataSourceType.Method)]
		public void ULongTestMethod(ulong value, byte[] serialized)
		{
			byte[] buf = new byte[serialized.Length];
			ulong readValue;
			int written, read;
			bool success;

			success=TryWriteUInt64VarInt(destination: buf, value: value, written: out written);
			Assert.IsTrue(success);
			Assert.AreEqual(expected: serialized.Length, actual: written);
			Assert.IsTrue(Enumerable.SequenceEqual(serialized, buf));

			success=TryReadUInt64VarInt(source: serialized, value: out readValue, read: out read);
			Assert.IsTrue(success);
			Assert.AreEqual(expected: serialized.Length, actual: read);
			Assert.AreEqual(expected: value, actual: readValue);
		}


		//public static IEnumerable<object[]> GetLongTestData()
		//{
		//	return new LongTest[]
		//	{
		//		new LongTest(0, new byte[] { 0x00, }),
		//		new LongTest(1, new byte[] { 0x01, }),
		//		new LongTest(-1, new byte[] { 0x7F, }),
		//		new LongTest(63, new byte[] { 0x3F, }),
		//		new LongTest(-64, new byte[] { 0x40, }),
		//		new LongTest(64, new byte[] { 0xC0, 0x00, }),
		//		new LongTest(-65, new byte[] { 0xBF, 0x7F, }),
		//		new LongTest(8191, new byte[] { 0xFF, 0x3F, }),
		//		new LongTest(-8192, new byte[] { 0x80, 0x40, }),
		//		new LongTest(8192, new byte[] { 0x80, 0xC0, 0x00, }),
		//		new LongTest(-8193, new byte[] { 0xFF, 0xBF, 0x7F, }),
		//		new LongTest(300, new byte[] { 0xAC, 0x02, }),
		//		new LongTest(long.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, }),
		//		new LongTest(long.MinValue, new byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x7F, }),
		//	}
		//	.Select(test => new object[] { test.Value, test.Serialized, });
		//}

		//public static IEnumerable<object[]> GetLongUTestData()
		//{
		//	return new ULongTest[]
		//	{
		//		new ULongTest(0, new byte[] { 0x00, }),
		//		new ULongTest(1, new byte[] { 0x01, }),
		//		new ULongTest(63, new byte[] { 0x3F, }),
		//		new ULongTest(64, new byte[] { 0xC0, 0x00, }),
		//		new ULongTest(127, new byte[] { 0xFF, 0x00, }),
		//		new ULongTest(128, new byte[] { 0x80, 0x01, }),
		//		new ULongTest(8191, new byte[] { 0xFF, 0x3F, }),
		//		new ULongTest(8192, new byte[] { 0x80, 0xC0, 0x00, }),
		//		new ULongTest(16383, new byte[] { 0xFF, 0xFF, 0x00, }),
		//		new ULongTest(16384, new byte[] { 0x80, 0x80, 0x01, }),
		//		new ULongTest(300, new byte[] { 0xAC, 0x02, }),
		//		new ULongTest(ulong.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, }),
		//	}
		//	.Select(test => new object[] { test.Value, test.Serialized, });
		//}

		public static IEnumerable<object[]> GetLongZigZagTestData()
		{
			return new LongTest[]
			{
				new LongTest(0, new byte[] { 0x00, }),
				new LongTest(1, new byte[] { 0x02, }),
				new LongTest(-1, new byte[] { 0x01, }),
				new LongTest(63, new byte[] { 0x7E, }),
				new LongTest(-64, new byte[] { 0x7F, }),
				new LongTest(64, new byte[] { 0x80, 0x01, }),
				new LongTest(-65, new byte[] { 0x81, 0x01, }),
				new LongTest(8191, new byte[] { 0xFE, 0x7F, }),
				new LongTest(-8192, new byte[] { 0xFF, 0x7F, }),
				new LongTest(8192, new byte[] { 0x80, 0x80, 0x01, }),
				new LongTest(-8193, new byte[] { 0x81, 0x80, 0x01, }),
				new LongTest(300, new byte[] { 0xD8, 0x04, }),
				new LongTest(int.MaxValue, new byte[] { 0xFE, 0xFF, 0xFF, 0xFF, 0x0F, }),
				new LongTest(int.MinValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x0F, }),
				new LongTest(long.MaxValue, new byte[] { 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, }),
				new LongTest(long.MinValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, }),
			}
			.Select(test => new object[] { test.Value, test.Serialized, });
		}

		public static IEnumerable<object[]> GetULongTestData()
		{
			return new ULongTest[]
			{
				new ULongTest(0, new byte[] { 0x00, }),
				new ULongTest(1, new byte[] { 0x01, }),
				new ULongTest(63, new byte[] { 0x3F, }),
				new ULongTest(64, new byte[] { 0x40, }),
				new ULongTest(127, new byte[] { 0x7F, }),
				new ULongTest(128, new byte[] { 0x80, 0x01, }),
				new ULongTest(16383, new byte[] { 0xFF, 0x7F, }),
				new ULongTest(16384, new byte[] { 0x80, 0x80, 0x01, }),
				new ULongTest(300, new byte[] { 0xAC, 0x02, }),
				new ULongTest(uint.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x0F, }),
				new ULongTest(ulong.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, }),
			}
			.Select(test => new object[] { test.Value, test.Serialized, });
		}
	}

	class LongTest
	{
		public long Value { get; set; }

		public byte[] Serialized { get; set; }

		public LongTest(long value, byte[] serialized)
		{
			this.Value=value;
			this.Serialized=serialized;
		}
	}

	class ULongTest
	{
		public ulong Value { get; set; }

		public byte[] Serialized { get; set; }

		public ULongTest(ulong value, byte[] serialized)
		{
			this.Value=value;
			this.Serialized=serialized;
		}
	}
}