using System;

namespace ProtoStream.Internal
{
	/// <summary>
	/// Klasa zawierająca metody do serializacji i deserializacji typów całkowitych o zmiennej długości (im dalej od zera tym więcej bajtów potrzeba do serializacji liczby)
	/// </summary>
	public static class Base64VarInt
	{
		///// <summary>
		///// Metoda próbuje zapisać 64-bitową liczbę ze znakiem (long) do tablicy bajtów
		///// </summary>
		///// <param name="destination">Tablica bajtów, gdzie serializujemy wartość</param>
		///// <param name="value">Wartość do zserializowania</param>
		///// <param name="written">Liczba zapisanych bajtów</param>
		///// <returns>Jeśli zapis się powiódł to true, w przeciwnym razie false - co oznacza że docelowa tablica miała za mało miejsca żeby zapisać liczbę</returns>
		//public static bool TryWriteInt64VarInt(Span<byte> destination, long value, out int written)
		//{
		//	long mask;
		//	byte val;

		//	written=0;
		//	mask=value>=0 ? 0 : -64;//-64 = 0xFFFFFFFFFFFFFFC0

		//	unchecked
		//	{
		//		while(written<destination.Length)
		//		{
		//			val=(byte)(value&0x7F);
		//			if(mask!=(value&-64L))//-64 = 0xFFFFFFFFFFFFFFC0
		//				val|=0x80;
		//			destination[written]=val;
		//			written++;
		//			if(0<=(sbyte)val)
		//				return true;
		//			value>>=7;
		//		}
		//		return false;
		//	}
		//}

		///// <summary>
		///// Metoda próbuje odczytać 64-bitową liczbę ze znakiem (long) z tablicy bajtów
		///// </summary>
		///// <param name="source">Tablica bajtów, z której deserializujemy wartość</param>
		///// <param name="value">Odczytana wartość</param>
		///// <param name="read">Liczba odczytanych bajtów</param>
		///// <returns>Jeśli odczyt się powiódł to true, w przeciwnym razie false - co oznacza że źródłowa tablica skończyła się zanim odczytano pełną wartość</returns>
		//public static bool TryReadInt64VarInt(ReadOnlySpan<byte> source, out long value, out int read)
		//{
		//	long mostSignificantBitMask = 0b0100_0000, nagativeComplementMask = -128;//-128 = 0xFFFFFFFFFFFFFF80
		//	int rotate = 0;
		//	byte val;

		//	read=0;
		//	value=0;

		//	unchecked
		//	{
		//		while(read<source.Length)
		//		{
		//			val=source[read];
		//			value|=(val&0x7FL)<<rotate;
		//			read++;

		//			if(0<=(sbyte)val)
		//			{
		//				//Sprawdź czy trzeba najbardziej znaczące bity (nie zapisane) uzupełnić jedynkami (jeśli liczba jest ujemna)
		//				mostSignificantBitMask<<=rotate;
		//				if(0!=(value&mostSignificantBitMask))
		//					value|=(nagativeComplementMask<<rotate);
		//				return true;
		//			}

		//			rotate+=7;
		//		}
		//		return false;
		//	}
		//}

		///// <summary>
		///// Metoda próbuje zapisać 64-bitową liczbę bez znaku (ulong) do tablicy bajtów
		///// </summary>
		///// <param name="destination">Tablica bajtów, gdzie serializujemy wartość</param>
		///// <param name="value">Wartość do zserializowania</param>
		///// <param name="written">Liczba zapisanych bajtów</param>
		///// <returns>Jeśli zapis się powiódł to true, w przeciwnym razie false - co oznacza że docelowa tablica miała za mało miejsca żeby zapisać liczbę</returns>
		//public static bool TryWriteInt64VarInt(Span<byte> destination, ulong value, out int written)
		//{
		//	byte val;

		//	written=0;

		//	unchecked
		//	{
		//		while(written<destination.Length)
		//		{
		//			val=(byte)(value&0x7F);
		//			if(0!=(value&0xFFFFFFFFFFFFFFC0UL))
		//				val|=0x80;
		//			destination[written]=val;
		//			written++;
		//			if(0<=(sbyte)val)
		//				return true;
		//			value>>=7;
		//		}
		//		return false;
		//	}
		//}

		///// <summary>
		///// Metoda próbuje odczytać 64-bitową liczbę bez znaku (ulong) z tablicy bajtów
		///// </summary>
		///// <param name="source">Tablica bajtów, z której deserializujemy wartość</param>
		///// <param name="value">Odczytana wartość</param>
		///// <param name="read">Liczba odczytanych bajtów</param>
		///// <returns>Jeśli odczyt się powiódł to true, w przeciwnym razie false - co oznacza że źródłowa tablica skończyła się zanim odczytano pełną wartość</returns>
		//public static bool TryReadInt64VarInt(ReadOnlySpan<byte> source, out ulong value, out int read)
		//{
		//	int rotate = 0;
		//	byte val;

		//	read=0;
		//	value=0;

		//	unchecked
		//	{
		//		while(read<source.Length)
		//		{
		//			val=source[read];
		//			value|=(val&0x7FUL)<<rotate;
		//			read++;

		//			if(0<=(sbyte)val)
		//				return true;

		//			rotate+=7;
		//		}
		//		return false;
		//	}
		//}

		/// <summary>
		/// Metoda próbuje zapisać 64-bitową liczbę ze znakiem (long) do tablicy bajtów
		/// </summary>
		/// <param name="destination">Tablica bajtów, gdzie serializujemy wartość</param>
		/// <param name="value">Wartość do zserializowania</param>
		/// <param name="written">Liczba zapisanych bajtów</param>
		/// <returns>Jeśli zapis się powiódł to true, w przeciwnym razie false - co oznacza że docelowa tablica miała za mało miejsca żeby zapisać liczbę</returns>
		public static bool TryWriteInt64VarInt(Span<byte> destination, long value, out int written)
		{
			return TryWriteUInt64VarInt(destination: destination, value: (ulong)((value<<1)^(value>>63)), written: out written);
		}

		/// <summary>
		/// Metoda próbuje zapisać 64-bitową liczbę bez znaku (ulong) do tablicy bajtów
		/// </summary>
		/// <param name="destination">Tablica bajtów, gdzie serializujemy wartość</param>
		/// <param name="value">Wartość do zserializowania</param>
		/// <param name="written">Liczba zapisanych bajtów</param>
		/// <returns>Jeśli zapis się powiódł to true, w przeciwnym razie false - co oznacza że docelowa tablica miała za mało miejsca żeby zapisać liczbę</returns>
		public static bool TryWriteUInt64VarInt(Span<byte> destination, ulong value, out int written)
		{
			byte val;

			written=0;

			unchecked
			{
				while(written<destination.Length)
				{
					val=(byte)(value&0x7F);
					if(0!=(value&0xFFFFFFFFFFFFFF80UL))
						val|=0x80;
					destination[written]=val;
					written++;
					if(0<=(sbyte)val)
						return true;
					value>>=7;
				}
				return false;
			}
		}

		/// <summary>
		/// Metoda próbuje odczytać 64-bitową liczbę ze znakiem (long) z tablicy bajtów
		/// </summary>
		/// <param name="source">Tablica bajtów, z której deserializujemy wartość</param>
		/// <param name="value">Odczytana wartość</param>
		/// <param name="read">Liczba odczytanych bajtów</param>
		/// <returns>Jeśli odczyt się powiódł to true, w przeciwnym razie false - co oznacza że źródłowa tablica skończyła się zanim odczytano pełną wartość</returns>
		public static bool TryReadInt64VarInt(ReadOnlySpan<byte> source, out long value, out int read)
		{
			ulong uValue;
			bool success;

			success=TryReadUInt64VarInt(source: source, value: out uValue, read: out read);
			value=(long)(uValue>>1)^-(long)(uValue&1);
			return success;
		}

		/// <summary>
		/// Metoda próbuje odczytać 64-bitową liczbę bez znaku (ulong) z tablicy bajtów
		/// </summary>
		/// <param name="source">Tablica bajtów, z której deserializujemy wartość</param>
		/// <param name="value">Odczytana wartość</param>
		/// <param name="read">Liczba odczytanych bajtów</param>
		/// <returns>Jeśli odczyt się powiódł to true, w przeciwnym razie false - co oznacza że źródłowa tablica skończyła się zanim odczytano pełną wartość</returns>
		public static bool TryReadUInt64VarInt(ReadOnlySpan<byte> source, out ulong value, out int read)
		{
			int rotate = 0;
			byte val;

			read=0;
			value=0;

			unchecked
			{
				while(read<source.Length)
				{
					val=source[read];
					value|=(val&0x7FUL)<<rotate;
					read++;

					if(0<=(sbyte)val)
						return true;

					rotate+=7;
				}
				return false;
			}
		}

		/// <summary>
		/// Metoda próbuje przeskoczyć pole typu Varint
		/// </summary>
		/// <param name="source">Tablica bajtów, z której deserializujemy dane</param>
		/// <param name="read">Liczba odczytanych bajtów</param>
		/// <returns>Jeśli przeskok powiódł się to true, w przeciwnym razie false - co oznacza że źródłowa tablica skończyła się zanim wykonano pełny przeskok</returns>
		public static bool TrySkipVarInt(ReadOnlySpan<byte> source, out int read)
		{
			byte val;

			read=0;

			while(read<source.Length)
			{
				val=source[read];
				read++;
				if(0<=(sbyte)source[read])
					return true;
			}
			return false;
		}
	}
}