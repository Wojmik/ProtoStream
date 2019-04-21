using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.PropertySerializers
{
	/// <summary>
	/// Delegacja zgodna z getterem
	/// </summary>
	/// <typeparam name="T">Typ, którego właściwość odczytujemy</typeparam>
	/// <typeparam name="TValue">Typ właściwości, którą odczytujemy</typeparam>
	/// <param name="obj">Obiekt, którego właściwość odczytujemy</param>
	/// <returns>Odczytana właściwość</returns>
	public delegate TValue GetHanlder<T, TValue>(T obj);
}