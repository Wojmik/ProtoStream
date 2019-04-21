using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ProtoStreamUnitTestCore.Model
{
	[DataContract]
	public class SampleClass
	{
		[DataMember(Order = 1)]
		public int Int1 { get; set; }

		[DataMember(Order = 2)]
		public uint UInt1 { get; set; }

		[DataMember(Order = 3)]
		public long Long1 { get; set; }

		[DataMember(Order = 4)]
		public ulong ULong1 { get; set; }

		[DataMember(Order = 5)]
		public string String1 { get; set; }

		//[DataMember(Order = 6)]
		//public byte[] ByteArray1 { get; set; }

		[DataMember(Order = 7)]
		public SampleNestedClass Nested1 { get; set; }

		[DataMember(Order = 11)]
		public sbyte SByte1 { get; set; }

		[DataMember(Order = 12)]
		public byte Byte1 { get; set; }

		[DataMember(Order = 13)]
		public short Short1 { get; set; }

		[DataMember(Order = 14)]
		public ushort UShort1 { get; set; }

		[DataMember(Order = 15)]
		public float Float1 { get; set; }

		[DataMember(Order = 16)]
		public double Double1 { get; set; }

		[DataMember(Order = 17)]
		public decimal Decimal1 { get; set; }

		[DataMember(Order = 18)]
		public bool Bool1 { get; set; }

		[DataMember(Order = 19)]
		public char Char1 { get; set; }

		[DataMember(Order = 20)]
		public DateTime DateTime1 { get; set; }

		[DataMember(Order = 21)]
		public TimeSpan TimeSpan1 { get; set; }

		[DataMember(Order = 22)]
		public DayOfWeek DayOfWeek1 { get; set; }

		[DataMember(Order = 51)]
		public int? IntN1 { get; set; }

		[DataMember(Order = 52)]
		public uint? UIntN1 { get; set; }

		[DataMember(Order = 53)]
		public long? LongN1 { get; set; }

		[DataMember(Order = 54)]
		public ulong? ULongN1 { get; set; }

		[DataMember(Order = 61)]
		public sbyte? SByteN1 { get; set; }

		[DataMember(Order = 62)]
		public byte? ByteN1 { get; set; }

		[DataMember(Order = 63)]
		public short? ShortN1 { get; set; }

		[DataMember(Order = 64)]
		public ushort? UShortN1 { get; set; }

		[DataMember(Order = 65)]
		public float? FloatN1 { get; set; }

		[DataMember(Order = 66)]
		public double? DoubleN1 { get; set; }

		[DataMember(Order = 67)]
		public decimal? DecimalN1 { get; set; }

		[DataMember(Order = 68)]
		public bool? BoolN1 { get; set; }

		[DataMember(Order = 69)]
		public char? CharN1 { get; set; }

		[DataMember(Order = 70)]
		public DateTime? DateTimeN1 { get; set; }

		[DataMember(Order = 71)]
		public TimeSpan? TimeSpanN1 { get; set; }

		[DataMember(Order = 72)]
		public DayOfWeek? DayOfWeekN1 { get; set; }


		public static void AreEqual(SampleClass expected, SampleClass actual)
		{
			Assert.AreEqual(expected.Int1, actual.Int1);
			Assert.AreEqual(expected.UInt1, actual.UInt1);
			Assert.AreEqual(expected.Long1, actual.Long1);
			Assert.AreEqual(expected.ULong1, actual.ULong1);
			Assert.AreEqual(expected.String1, actual.String1);
			//Assert.AreEqual(expected.ByteArray1, actual.ByteArray1);
			Assert.AreEqual(expected.SByte1, actual.SByte1);
			Assert.AreEqual(expected.Byte1, actual.Byte1);
			Assert.AreEqual(expected.Short1, actual.Short1);
			Assert.AreEqual(expected.UShort1, actual.UShort1);
			Assert.AreEqual(expected.Float1, actual.Float1);
			Assert.AreEqual(expected.Double1, actual.Double1);
			Assert.AreEqual(expected.Decimal1, actual.Decimal1);
			Assert.AreEqual(expected.Bool1, actual.Bool1);
			Assert.AreEqual(expected.Char1, actual.Char1);
			Assert.AreEqual(expected.DateTime1, actual.DateTime1);
			Assert.AreEqual(expected.TimeSpan1, actual.TimeSpan1);
			Assert.AreEqual(expected.DayOfWeek1, actual.DayOfWeek1);
			Assert.AreEqual(expected.IntN1, actual.IntN1);
			Assert.AreEqual(expected.UIntN1, actual.UIntN1);
			Assert.AreEqual(expected.LongN1, actual.LongN1);
			Assert.AreEqual(expected.ULongN1, actual.ULongN1);
			Assert.AreEqual(expected.SByteN1, actual.SByteN1);
			Assert.AreEqual(expected.ByteN1, actual.ByteN1);
			Assert.AreEqual(expected.ShortN1, actual.ShortN1);
			Assert.AreEqual(expected.UShortN1, actual.UShortN1);
			Assert.AreEqual(expected.FloatN1, actual.FloatN1);
			Assert.AreEqual(expected.DoubleN1, actual.DoubleN1);
			Assert.AreEqual(expected.DecimalN1, actual.DecimalN1);
			Assert.AreEqual(expected.BoolN1, actual.BoolN1);
			Assert.AreEqual(expected.CharN1, actual.CharN1);
			Assert.AreEqual(expected.DateTimeN1, actual.DateTimeN1);
			Assert.AreEqual(expected.TimeSpanN1, actual.TimeSpanN1);
			Assert.AreEqual(expected.DayOfWeekN1, actual.DayOfWeekN1);

			if(expected.Nested1!=null && actual.Nested1!=null)
				SampleNestedClass.AreEqual(expected: expected.Nested1, actual: actual.Nested1);
			else
				Assert.AreEqual(expected.Nested1!=null, actual.Nested1!=null);
		}
	}
}