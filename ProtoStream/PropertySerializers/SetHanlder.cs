using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.PropertySerializers
{
	/// <summary>
	/// Delegacja zgodna z setterem
	/// </summary>
	/// <typeparam name="T">Typ, którego właściwość ustawiamy</typeparam>
	/// <typeparam name="TValue">Typ właściwości, którą ustawiamy</typeparam>
	/// <param name="obj">Obiekt, którego właściwość ustawiamy</param>
	/// <param name="val">Wartość, którą ustawiamy</param>
	public delegate void SetHanlder<T, TValue>(T obj, TValue val);
}