using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.Internal
{
	/// <summary>
	/// Protokół niskopoziomowy
	/// </summary>
	public static class WireProtocol
	{
		static readonly SkipMethodHandler[] SkipMethodsByWireType = new SkipMethodHandler[]
		{
			Base64VarInt.TrySkipVarInt,
			TrySkipFixed64,
			TrySkipLengthDelimited,
			TrySkipStartGroup,
			TrySkipEndGroup,
			TrySkipFixed32,
			TrySkipUnknown6,
			TrySkipUnknown7,
		};

		#region Metody zapisujące i odczytujące typ pola i numer pola
		/// <summary>
		/// Metoda próbuje zapisać typ pola i numer pola do tablicy bajtów
		/// </summary>
		/// <param name="destination">Tablica bajtów, gdzie serializujemy dane</param>
		/// <param name="type">Typ pola</param>
		/// <param name="fieldNo">Nr pola</param>
		/// <param name="written">Liczba zapisanych bajtów</param>
		/// <returns>Jeśli zapis się powiódł to true, w przeciwnym razie false - co oznacza że docelowa tablica miała za mało miejsca żeby zapisać dane</returns>
		public static bool TryWriteFieldKey(Span<byte> destination, WireType type, int fieldNo, out int written)
		{
			return Base64VarInt.TryWriteUInt64VarInt(destination: destination, value: (((ulong)fieldNo)<<3)|(ulong)type, written: out written);
		}

		/// <summary>
		/// Metoda próbuje odczytać typ pola i numer pola z tablicy bajtów
		/// </summary>
		/// <param name="source">Tablica bajtów, z której deserializujemy dane</param>
		/// <param name="type">Odczytany typ pola</param>
		/// <param name="fieldNo">Odczytany nr pola</param>
		/// <param name="read">Liczba odczytanych bajtów</param>
		/// <returns>Jeśli odczyt się powiódł to true, w przeciwnym razie false - co oznacza że źródłowa tablica skończyła się zanim odczytano pełne dane</returns>
		public static bool TryReadFieldKey(ReadOnlySpan<byte> source, out WireType type, out int fieldNo, out int read)
		{
			ulong value;
			bool success;

			success=Base64VarInt.TryReadUInt64VarInt(source: source, value: out value, read: out read);
			type=(WireType)(value&0x07);
			fieldNo=(int)(value>>3);
			return success;
		}
		#endregion

		#region Metody przeskakujące
		/// <summary>
		/// Metoda próbuje przeskoczyć pole określonego typu
		/// </summary>
		/// <param name="source">Tablica bajtów, z której deserializujemy dane</param>
		/// <param name="type">Typ pola</param>
		/// <param name="read">Liczba odczytanych bajtów</param>
		/// <returns>Jeśli przeskok powiódł się to true, w przeciwnym razie false - co oznacza że źródłowa tablica skończyła się zanim wykonano pełny przeskok</returns>
		public static bool TrySkipField(ReadOnlySpan<byte> source, WireType type, out int read)
		{
			return SkipMethodsByWireType[(int)type](source: source, read: out read);
		}
		
		/// <summary>
		/// Metoda próbuje przeskoczyć pole typu Fixed64
		/// </summary>
		/// <param name="source">Tablica bajtów, z której deserializujemy dane</param>
		/// <param name="read">Liczba odczytanych bajtów</param>
		/// <returns>Jeśli przeskok powiódł się to true, w przeciwnym razie false - co oznacza że źródłowa tablica skończyła się zanim wykonano pełny przeskok</returns>
		public static bool TrySkipFixed64(ReadOnlySpan<byte> source, out int read)
		{
			bool success;

			success=(sizeof(long)<=source.Length);

			read=success ? sizeof(long) : source.Length;
			return success;
		}

		/// <summary>
		/// Metoda próbuje przeskoczyć pole typu LengthDelimited
		/// </summary>
		/// <param name="source">Tablica bajtów, z której deserializujemy dane</param>
		/// <param name="read">Liczba odczytanych bajtów</param>
		/// <returns>Jeśli przeskok powiódł się to true, w przeciwnym razie false - co oznacza że źródłowa tablica skończyła się zanim wykonano pełny przeskok</returns>
		public static bool TrySkipLengthDelimited(ReadOnlySpan<byte> source, out int read)
		{
			long bytesToRead;
			bool success;

			//Odczytaj długość pola zmiennej długości
			success=Base64VarInt.TryReadInt64VarInt(source: source, value: out bytesToRead, read: out read);
			//Pomiń tyle bajtów ile zajmuje pole
			if(success)
			{
				success=(read+(int)bytesToRead<=source.Length);
				read=success ? read+(int)bytesToRead : source.Length;
			}
			return success;
		}

		/// <summary>
		/// Metoda próbuje przeskoczyć pole typu StartGroup
		/// </summary>
		/// <param name="source">Tablica bajtów, z której deserializujemy dane</param>
		/// <param name="read">Liczba odczytanych bajtów</param>
		/// <returns>Jeśli przeskok powiódł się to true, w przeciwnym razie false - co oznacza że źródłowa tablica skończyła się zanim wykonano pełny przeskok</returns>
		public static bool TrySkipStartGroup(ReadOnlySpan<byte> source, out int read)
		{
			read=0;
			return true;
		}

		/// <summary>
		/// Metoda próbuje przeskoczyć pole typu EndGroup
		/// </summary>
		/// <param name="source">Tablica bajtów, z której deserializujemy dane</param>
		/// <param name="read">Liczba odczytanych bajtów</param>
		/// <returns>Jeśli przeskok powiódł się to true, w przeciwnym razie false - co oznacza że źródłowa tablica skończyła się zanim wykonano pełny przeskok</returns>
		public static bool TrySkipEndGroup(ReadOnlySpan<byte> source, out int read)
		{
			read=0;
			return true;
		}

		/// <summary>
		/// Metoda próbuje przeskoczyć pole typu Fixed32
		/// </summary>
		/// <param name="source">Tablica bajtów, z której deserializujemy dane</param>
		/// <param name="read">Liczba odczytanych bajtów</param>
		/// <returns>Jeśli przeskok powiódł się to true, w przeciwnym razie false - co oznacza że źródłowa tablica skończyła się zanim wykonano pełny przeskok</returns>
		public static bool TrySkipFixed32(ReadOnlySpan<byte> source, out int read)
		{
			bool success;

			success=(sizeof(int)<=source.Length);

			read=success ? sizeof(int) : source.Length;
			return success;
		}

		/// <summary>
		/// Metoda próbuje przeskoczyć pole typu Unknown6
		/// </summary>
		/// <param name="source">Tablica bajtów, z której deserializujemy dane</param>
		/// <param name="read">Liczba odczytanych bajtów</param>
		/// <returns>Jeśli przeskok powiódł się to true, w przeciwnym razie false - co oznacza że źródłowa tablica skończyła się zanim wykonano pełny przeskok</returns>
		public static bool TrySkipUnknown6(ReadOnlySpan<byte> source, out int read)
		{
			throw new NotSupportedException($"Not supported field type: {nameof(WireType.Unknown6)} ({(int)WireType.Unknown6})");
		}

		/// <summary>
		/// Metoda próbuje przeskoczyć pole typu Unknown7
		/// </summary>
		/// <param name="source">Tablica bajtów, z której deserializujemy dane</param>
		/// <param name="read">Liczba odczytanych bajtów</param>
		/// <returns>Jeśli przeskok powiódł się to true, w przeciwnym razie false - co oznacza że źródłowa tablica skończyła się zanim wykonano pełny przeskok</returns>
		public static bool TrySkipUnknown7(ReadOnlySpan<byte> source, out int read)
		{
			throw new NotSupportedException($"Not supported field type: {nameof(WireType.Unknown7)} ({(int)WireType.Unknown7})");
		}
		#endregion
	}
}