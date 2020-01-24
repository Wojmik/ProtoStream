using ProtoStream.PropertySerializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoStream.CollectionSerializers
{
	public class DictionarySerializer<TDictionary, TKey, TValue> : ISerializer<TDictionary>
		where TDictionary : IDictionary<TKey, TValue>
	{
		/// <summary>
		/// Method creating new instance of TList
		/// </summary>
		protected Model.NewInctanceHandler<TDictionary> NewDictionaryFunc { get; }

		/// <summary>
		/// Index serializer
		/// </summary>
		protected ISerializer<TKey> KeySerializer { get; }

		/// <summary>
		/// Items serializer
		/// </summary>
		protected ISerializer<TValue> ValueSerializer { get; }

		public DictionarySerializer(SerializationType serializationType, bool packed, Model.NewInctanceHandler<TDictionary> newDictionaryFunc, Dictionary<Type, Serializer> typeSerializers)
		{
			this.NewDictionaryFunc=newDictionaryFunc;

			KeySerializer=Serializer.GetSerializer<TKey>(serializationType: serializationType, packed: packed, typeSerializers: typeSerializers);

			ValueSerializer=Serializer.GetSerializer<TValue>(serializationType: serializationType, packed: packed, typeSerializers: typeSerializers);
		}

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="fieldNo">Unique field no</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public async ValueTask SerializeAsync(ProtoStreamWriter writer, int fieldNo, TDictionary value, CancellationToken cancellationToken = default)
		{
			int i = 0;

			if(value!=null)
				foreach(var item in value)
				{
					//Enter opaque object
					await writer.EnterObjectAsync(fieldNo: fieldNo, cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					//Key
					await KeySerializer.SerializeAsync(writer: writer, fieldNo: 1, value: item.Key, cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					//Value
					await ValueSerializer.SerializeAsync(writer: writer, fieldNo: 2, value: item.Value, cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					//Leave opaque object
					await writer.LeaveObjectAsync(cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					i++;
				}
		}

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="previousValue">So far read value</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public async ValueTask<TDictionary> DeserializeAsync(ProtoStreamReader reader, TDictionary previousValue, CancellationToken cancellationToken = default)
		{
			FieldHeader fieldHeader;
			TValue value;
			int collectionFieldNo;
			UserData userData;

			if(previousValue==null)
				previousValue=NewDictionaryFunc();

			fieldHeader=reader.CurrentFieldHeader;
			collectionFieldNo=fieldHeader.FieldNo;

			//Check opaque object wire type
			if(fieldHeader.WireType!=Internal.WireType.LengthDelimited)
				throw new SerializationException($"Unexpected wire type for map opaque, field no: {fieldHeader.FieldNo}. Wire type: {fieldHeader.WireType} and should be {Internal.WireType.LengthDelimited}");

			//Get or create user data attached to this object
			userData=(UserData)reader.GetOrCreateUserData(() => new UserData() { Key=default, ResetKey=true, });

			while(true)
			{
				//Read header
				fieldHeader=await reader.ReadFieldHeaderAsync(cancellationToken: cancellationToken)
					.ConfigureAwait(false);

				switch(fieldHeader.FieldNo)
				{
					case 1://Dictionary key
						try
						{
							if(userData.ResetKey)
							{
								userData.Key=default;
								userData.ResetKey=false;
							}
							userData.Key=await KeySerializer.DeserializeAsync(reader: reader, previousValue: userData.Key, cancellationToken: cancellationToken)
								.ConfigureAwait(false);
						}
						catch(SerializationException ex)
						{
							throw new SerializationException($"Cannot deserialize map key", ex);
						}
						break;
					case 2://Dictionary Value
						userData.ResetKey=true;
						if(!previousValue.TryGetValue(userData.Key, out value))//Get object on specified key
							previousValue.Add(userData.Key, value=default);

						value=await ValueSerializer.DeserializeAsync(reader: reader, previousValue: value, cancellationToken: cancellationToken)
							.ConfigureAwait(false);
						previousValue[userData.Key]=value;
						break;
					default:
						if(fieldHeader.FieldNo<=(int)Model.FieldNoEvent.EndGroup)//Group end, leave nested object, end of transmission
							return previousValue;
						else
							throw new SerializationException($"Unexpected field no: {fieldHeader.FieldNo} in map opaque obiect. Map field no: {collectionFieldNo}");
				}
			}
		}

		/// <summary>
		/// Is <paramref name="value"/> default value
		/// </summary>
		/// <param name="value">Value to check</param>
		/// <returns>True if <paramref name="value"/> is default value, false otherwise</returns>
		public bool IsDefault(TDictionary value)
		{
			return value==null;
		}

		class UserData
		{
			public TKey Key;
			public bool ResetKey;
		}
	}
}