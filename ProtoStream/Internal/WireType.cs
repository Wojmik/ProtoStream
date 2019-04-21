using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.Internal
{
	/// <summary>
	/// Typ pola
	/// </summary>
	public enum WireType
	{
		/// <summary>
		/// Liczba całkowita o zmiennej długości
		/// </summary>
		VarInt = 0,

		/// <summary>
		/// Liczba 64-bitowa - zapisana na ośmiu bajtach
		/// </summary>
		Fixed64 = 1,

		/// <summary>
		/// Ciąg bajtów o podanej długości
		/// </summary>
		LengthDelimited = 2,

		/// <summary>
		/// Rozpoczęcie grupy
		/// </summary>
		StartGroup = 3,

		/// <summary>
		/// Zakończenie grupy
		/// </summary>
		EndGroup = 4,

		/// <summary>
		/// Liczba 32-bitowa - zapisana na czterech bajtach
		/// </summary>
		Fixed32 = 5,

		/// <summary>
		/// Nieznany typ
		/// </summary>
		Unknown6 = 6,

		/// <summary>
		/// Nieznany typ
		/// </summary>
		Unknown7 = 7,
	}
}