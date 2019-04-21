using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoStream.PropertySerializers
{
	/// <summary>
	/// Property serializer settings
	/// </summary>
	public readonly struct PropertySerializerSettings
	{
		/// <summary>
		/// Unique field no
		/// </summary>
		public int FieldNo { get; }

		/// <summary>
		/// How property should be serialized
		/// </summary>
		public SerializationType SerializationType { get; }

		/// <summary>
		/// Is repetable fields shouldn't be packed on serialization
		/// </summary>
		public bool NoPacked { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="fieldNo">Unique field no</param>
		/// <param name="serializationType">How property should be serialized</param>
		/// <param name="noPacked">Is repetable fields shouldn't be packed on serialization</param>
		public PropertySerializerSettings(int fieldNo, SerializationType serializationType, bool noPacked)
		{
			this.FieldNo=fieldNo;
			this.SerializationType=serializationType;
			this.NoPacked=noPacked;
		}
	}
}