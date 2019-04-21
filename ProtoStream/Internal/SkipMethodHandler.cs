using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.Internal
{
	/// <summary>
	/// Definicja metody próbującej przeskoczyć pole
	/// </summary>
	/// <param name="source">Tablica bajtów, z której deserializujemy dane</param>
	/// <param name="read">Liczba odczytanych bajtów</param>
	/// <returns>Jeśli przeskok powiódł się to true, w przeciwnym razie false - co oznacza że źródłowa tablica skończyła się zanim wykonano pełny przeskok</returns>
	delegate bool SkipMethodHandler(ReadOnlySpan<byte> source, out int read);
}