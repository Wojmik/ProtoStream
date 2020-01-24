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
	public class ListSerializer<TList, TItem> : ISerializer<TList>
		where TList : IList<TItem>
	{
		/// <summary>
		/// Is collection should be packed when serializing
		/// </summary>
		public virtual bool Packed { get; }

		/// <summary>
		/// Method creating new instance of TList
		/// </summary>
		protected Model.NewInctanceHandler<TList> NewListFunc { get; }

		/// <summary>
		/// Index serializer
		/// </summary>
		protected TypeSerializers.SerializerBase<uint> IndexSerializer { get; }

		/// <summary>
		/// Items serializer
		/// </summary>
		protected ISerializer<TItem> ItemSerializer { get; }

		public ListSerializer(SerializationType serializationType, bool packed, Model.NewInctanceHandler<TList> newListFunc, Dictionary<Type, Serializer> typeSerializers)
		{
			this.Packed=packed;
			this.NewListFunc=newListFunc;

			IndexSerializer=TypeSerializers.VarUIntSerializer.Default;

			//Get item serializer
			ItemSerializer=Serializer.GetSerializer<TItem>(serializationType: serializationType, packed: packed, typeSerializers: typeSerializers);
		}

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="fieldNo">Unique field no</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public async ValueTask SerializeAsync(ProtoStreamWriter writer, int fieldNo, TList value, CancellationToken cancellationToken = default)
		{
			int i = 0;

			if(value!=null)
				if(ItemSerializer is ISerializerValue<TItem> itemPackedSerializer)//Does ItemSerializer support packed collections
					if(Packed)//Packed
					{
						await writer.EnterObjectAsync(fieldNo: fieldNo, cancellationToken: cancellationToken)
							.ConfigureAwait(false);

						foreach(var item in value)
							await itemPackedSerializer.SerializeValueAsync(writer: writer, value: item, cancellationToken: cancellationToken)
								.ConfigureAwait(false);

						await writer.LeaveObjectAsync(cancellationToken: cancellationToken)
							.ConfigureAwait(false);
					}
					else//Simple items not packed
					{
						foreach(var item in value)
							await ItemSerializer.SerializeAsync(writer: writer, fieldNo: fieldNo, value: item, cancellationToken: cancellationToken)
								.ConfigureAwait(false);
					}
				else//Not packed
				{
					foreach(var item in value)
					{
						//Enter opaque object
						await writer.EnterObjectAsync(fieldNo: fieldNo, cancellationToken: cancellationToken)
							.ConfigureAwait(false);

						//Index
						await IndexSerializer.SerializeAsync(writer: writer, fieldNo: 1, value: (uint)i, cancellationToken: cancellationToken)
							.ConfigureAwait(false);

						//Value
						if(item!=null)
						{
							await ItemSerializer.SerializeAsync(writer: writer, fieldNo: 2, value: item, cancellationToken: cancellationToken)
								.ConfigureAwait(false);
						}

						//Leave opaque object
						await writer.LeaveObjectAsync(cancellationToken: cancellationToken)
							.ConfigureAwait(false);

						i++;
					}
				}
		}

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="previousValue">So far read value</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public async ValueTask<TList> DeserializeAsync(ProtoStreamReader reader, TList previousValue, CancellationToken cancellationToken = default)
		{
			FieldHeader fieldHeader;
			ValueWithSize<TItem> valueWithSize;
			TItem item;
			int read = 0, index, collectionFieldNo;

			if(previousValue==null)
				previousValue=NewListFunc();

			fieldHeader=reader.CurrentFieldHeader;
			collectionFieldNo=fieldHeader.FieldNo;
			if(ItemSerializer is ISerializerValue<TItem> itemPackedSerializer)//Does ItemSerializer support packed collections
				if(fieldHeader.WireType==Internal.WireType.LengthDelimited)//Packed
				{
					while(read<fieldHeader.FieldLength)
					{
						valueWithSize=await itemPackedSerializer.DeserializeValueAsync(reader: reader, cancellationToken: cancellationToken)
							.ConfigureAwait(false);
						previousValue.Add(valueWithSize.Value);
						read+=valueWithSize.Size;
					}

					//Check if field length coresponds with field data
					if(read>fieldHeader.FieldLength)
						throw new SerializationException("Packed repeated field length doesn't corespond with filed data");

					//Read leaving nested object
					fieldHeader=await reader.ReadFieldHeaderAsync(cancellationToken: cancellationToken)
						.ConfigureAwait(false);
					if(fieldHeader.FieldNo!=(int)Model.FieldNoEvent.LeavingNestedObject)
						throw new SerializationException($"Should be leaving nested object and is: {fieldHeader.WireType}");
				}
				else//Simple items not packed
				{
					item=await ItemSerializer.DeserializeAsync(reader: reader, previousValue: default, cancellationToken: cancellationToken)
						.ConfigureAwait(false);
					previousValue.Add(item);
				}
			else//Not packed
			{
				//Check opaque object wire type
				if(fieldHeader.WireType!=Internal.WireType.LengthDelimited)
					throw new SerializationException($"Unexpected wire type for collection opaque, field no: {fieldHeader.FieldNo}. Wire type: {fieldHeader.WireType} and should be {Internal.WireType.LengthDelimited}");

				while(true)
				{
					//Read header
					fieldHeader=await reader.ReadFieldHeaderAsync(cancellationToken: cancellationToken)
						.ConfigureAwait(false);

					switch(fieldHeader.FieldNo)
					{
						case 1://Collection index
							try
							{
								index=(int)await IndexSerializer.DeserializeAsync(reader: reader, previousValue: default, cancellationToken: cancellationToken)
									.ConfigureAwait(false);
							}
							catch(SerializationException ex)
							{
								throw new SerializationException($"Cannot deserialize collection index", ex);
							}

							//Get object on specified index
							if(previousValue.Count<=index)
							{
								previousValue.Add(default);
								if(previousValue.Count<=index)
									throw new SerializationException($"Unexpected collection index. Collection field no: {collectionFieldNo}. Expected index: {previousValue.Count-1} and is: {index}");
							}
							break;
						case 2://collection Value
							if(previousValue.Count<=0)
								throw new SerializationException($"Collection value send before collection index. Collection field no: {collectionFieldNo}");
							item=previousValue[previousValue.Count-1];
							item=await ItemSerializer.DeserializeAsync(reader: reader, previousValue: item, cancellationToken: cancellationToken)
								.ConfigureAwait(false);
							previousValue[previousValue.Count-1]=item;
							break;
						default:
							if(fieldHeader.FieldNo<=(int)Model.FieldNoEvent.EndGroup)//Group end, leave nested object, end of transmission
								return previousValue;
							else
								throw new SerializationException($"Unexpected field no: {fieldHeader.FieldNo} in collection opaque obiect. Collection field no: {collectionFieldNo}");
					}
				}
			}

			return previousValue;
		}

		/// <summary>
		/// Is <paramref name="value"/> default value
		/// </summary>
		/// <param name="value">Value to check</param>
		/// <returns>True if <paramref name="value"/> is default value, false otherwise</returns>
		public bool IsDefault(TList value)
		{
			return value==null;
		}
	}
}