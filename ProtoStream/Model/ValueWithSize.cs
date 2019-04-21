using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream
{
	/// <summary>
	/// Value with size
	/// </summary>
	/// <typeparam name="T">Value type</typeparam>
	public struct ValueWithSize<T>
	{
		/// <summary>
		/// Value
		/// </summary>
		public T Value;

		/// <summary>
		/// Size of value in bytes
		/// </summary>
		public int Size;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="value">Value</param>
		/// <param name="size">Size of value in bytes</param>
		public ValueWithSize(T value, int size)
		{
			this.Value=value;
			this.Size=size;
		}
	}
}