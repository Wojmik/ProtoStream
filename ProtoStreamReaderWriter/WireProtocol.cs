using System;
using System.Collections.Generic;
using System.Text;

namespace WojciechMikołajewicz.ProtoStreamReaderWriter
{
	/// <summary>
	/// Serializer / Deserializer on wire level
	/// </summary>
	public static class WireProtocol
	{
		/// <summary>
		/// Tries to write field header to <paramref name="destination"/>
		/// </summary>
		/// <param name="destination">Span to write to</param>
		/// <param name="wireType">Wire type</param>
		/// <param name="fieldNo">Field number</param>
		/// <param name="written">Number of bytes written to the <paramref name="destination"/></param>
		/// <returns>True if success or false if not - which means there was not sufficient space in <paramref name="destination"/> to write field header</returns>
		/// <exception cref="ArgumentException">Wrong value in <paramref name="wireType"/> or negative fieldNo</exception>
		public static bool TryWriteFieldHeader(Span<byte> destination, WireType wireType, int fieldNo, out int written)
		{
			if(fieldNo<0)
				throw new ArgumentException($"{nameof(fieldNo)} cannot be negative", nameof(fieldNo));
			if(0!=((int)wireType&-8))//-8 = 0xFFFFFFF8
				throw new ArgumentException($"Unexpected wire type: {wireType}", nameof(wireType));

			return Base128.TryWriteUInt64(destination: destination, value: (ulong)fieldNo<<3|(ulong)wireType, written: out written);
		}

		/// <summary>
		/// Tries to write length delimited field header to <paramref name="destination"/>
		/// </summary>
		/// <param name="destination">Span to write to</param>
		/// <param name="fieldNo">Field number</param>
		/// <param name="fieldLength">Length of the field</param>
		/// <param name="written">Number of bytes written to the <paramref name="destination"/></param>
		/// <returns>True if success or false if not - which means there was not sufficient space in <paramref name="destination"/> to write field header</returns>
		/// <exception cref="ArgumentException">Negative fieldNo or fieldLength</exception>
		public static bool TryWriteLengthDelimitedFieldHeader(Span<byte> destination, int fieldNo, long fieldLength, out int written)
		{
			int wrtn;

			if(fieldNo<0)
				throw new ArgumentException($"{nameof(fieldNo)} cannot be negative", nameof(fieldNo));
			if(fieldLength<0)
				throw new ArgumentException($"{nameof(fieldLength)} cannot be negative", nameof(fieldLength));

			if(!Base128.TryWriteUInt64(destination: destination, value: (ulong)fieldNo<<3|(ulong)WireType.LengthDelimited, written: out written))
				return false;
			if(!Base128.TryWriteUInt64(destination: destination.Slice(written), value: (ulong)fieldLength, written: out wrtn))
				return false;
			
			written+=wrtn;
			return true;
		}

		/// <summary>
		/// Tries to read field header from <paramref name="source"/>
		/// </summary>
		/// <param name="source">Span to read from</param>
		/// <param name="fieldHeader">Read fieald header</param>
		/// <param name="read">Number of bytes read from the <paramref name="source"/></param>
		/// <returns>True if success or false if not - which means end of <paramref name="source"/> was reached before whole field header was read</returns>
		public static bool TryReadFieldHeader(ReadOnlySpan<byte> source, out WireFieldHeaderData fieldHeader, out int read)
		{
			ulong headerValue;
			bool ok;

			ok=Base128.TryReadUInt64(source: source, value: out headerValue, read: out read);
			fieldHeader=new WireFieldHeaderData(wireType: (WireType)((int)headerValue&0x7), fieldNo: (int)(headerValue>>3));
			return ok;
		}
	}
}