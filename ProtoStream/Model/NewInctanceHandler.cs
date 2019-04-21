using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.Model
{
	/// <summary>
	/// Method definition for create new instance of object <typeparamref name="T"/>
	/// </summary>
	/// <typeparam name="T">Type of object creating</typeparam>
	/// <returns>New instance of object <typeparamref name="T"/></returns>
	public delegate T NewInctanceHandler<T>();
}