using ProtoStream.PropertySerializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoStream
{
	/// <summary>
	/// Base class for serializers
	/// </summary>
	public abstract class Serializer
	{
		//public abstract Task SerializeAsync(ProtoStreamWriter protoStreamWriter, object obj, CancellationToken cancellationToken = default);

		public static ISerializer<T> GetSerializer<T>(SerializationType serializationType, bool packed, Dictionary<Type, Serializer> typeSerializers)
		{
			ISerializer<T> serializer;
			Type genericType;

			//Get item serializer
			if(null==(serializer=TypeSerializers.SerializerBase<T>.CreateSerializer(serializationType: serializationType)))
				if(typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition()==typeof(IDictionary<,>))//IDictionary<,>
					serializer=(ISerializer<T>)Activator.CreateInstance(typeof(CollectionSerializers.DictionarySerializer<,,>).MakeGenericType(typeof(T), typeof(T).GenericTypeArguments[0], typeof(T).GenericTypeArguments[1]), serializationType, packed, CreateDictionaryDelegate(typeof(T)), typeSerializers);
				else if(null!=(genericType=typeof(T).GetInterfaces().FirstOrDefault(tp => tp.IsGenericType && tp.GetGenericTypeDefinition()==typeof(IDictionary<,>))))//Implements IDictionary<,>
					serializer=(ISerializer<T>)Activator.CreateInstance(typeof(CollectionSerializers.DictionarySerializer<,,>).MakeGenericType(typeof(T), genericType.GenericTypeArguments[0], genericType.GenericTypeArguments[1]), serializationType, packed, CreateDictionaryDelegate(typeof(T)), typeSerializers);
				else if(typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition()==typeof(IList<>))//IList<>
					serializer=(ISerializer<T>)Activator.CreateInstance(typeof(CollectionSerializers.ListSerializer<,>).MakeGenericType(typeof(T), typeof(T).GenericTypeArguments[0]), serializationType, packed, CreateListDelegate(typeof(T)), typeSerializers);
				else if(null!=(genericType=typeof(T).GetInterfaces().FirstOrDefault(tp => tp.IsGenericType && tp.GetGenericTypeDefinition()==typeof(IList<>))))//Implements IList<>
					serializer=(ISerializer<T>)Activator.CreateInstance(typeof(CollectionSerializers.ListSerializer<,>).MakeGenericType(typeof(T), genericType.GenericTypeArguments[0]), serializationType, packed, CreateListDelegate(typeof(T)), typeSerializers);
				else//Nested object
					serializer=(ISerializer<T>)Activator.CreateInstance(typeof(Serializer<>).MakeGenericType(typeof(T)), typeSerializers);

			return serializer;
		}

		protected static Delegate CreateListDelegate(Type type)
		{
			return Delegate.CreateDelegate(typeof(Model.NewInctanceHandler<>).MakeGenericType(type),
				typeof(Serializer).GetMethod(nameof(CreateList), BindingFlags.Static|BindingFlags.NonPublic).MakeGenericMethod(type));
		}

		protected static T CreateList<T>()
		{
			return (T)Activator.CreateInstance(typeof(List<>).MakeGenericType(typeof(T).GenericTypeArguments[0]));
		}

		protected static Delegate CreateDictionaryDelegate(Type type)
		{
			return Delegate.CreateDelegate(typeof(Model.NewInctanceHandler<>).MakeGenericType(type),
				typeof(Serializer).GetMethod(nameof(CreateDictionary), BindingFlags.Static|BindingFlags.NonPublic).MakeGenericMethod(type));
		}

		protected static T CreateDictionary<T>()
		{
			return (T)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(typeof(T).GenericTypeArguments[0], typeof(T).GenericTypeArguments[1]));
		}

		/// <summary>
		/// Serialize object
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="value">Object to serialize</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public abstract ValueTask SerializeAsync(ProtoStreamWriter writer, object value, CancellationToken cancellationToken = default);

		/// <summary>
		/// Deserialize to object
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="previousValue">Previously read object value</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Deserialized object</returns>
		public abstract ValueTask<object> DeserializeAsync(ProtoStreamReader reader, object previousValue = default, CancellationToken cancellationToken = default);
	}

	/// <summary>
	/// Type serializer
	/// </summary>
	/// <typeparam name="T">Object type for serialization / deserialization</typeparam>
	public class Serializer<T> : Serializer, ISerializer<T>
		where T : class, new()
	{
		/// <summary>
		/// Type serializers
		/// </summary>
		protected Dictionary<Type, Serializer> TypeSerializers { get; }

		/// <summary>
		/// Property serializers
		/// </summary>
		protected List<PropertySerializer<T>> PropertySerializers { get; }

		/// <summary>
		/// Property serializers by FieldNo
		/// </summary>
		protected Dictionary<int, PropertySerializer<T>> PropertySerializersDictionary { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="typeSerializers">Dictionary of types serializers</param>
		public Serializer(Dictionary<Type, Serializer> typeSerializers)
		{
			this.TypeSerializers=typeSerializers;
			this.PropertySerializers=new List<PropertySerializer<T>>();
			this.PropertySerializersDictionary=new Dictionary<int, PropertySerializer<T>>();
			
			CreateTypeMap();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public Serializer()
			: this(typeSerializers: new Dictionary<Type, Serializer>())
		{ }

		/// <summary>
		/// Serialize object with header
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="fieldNo">Unique field no</param>
		/// <param name="value">Object to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public virtual async ValueTask SerializeAsync(ProtoStreamWriter writer, int fieldNo, T value, CancellationToken cancellationToken)
		{
			await writer.EnterObjectAsync(fieldNo: fieldNo, cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			await SerializeAsync(writer: writer, value: value, cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			await writer.LeaveObjectAsync(cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Serialize object
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="value">Object to serialize</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public override ValueTask SerializeAsync(ProtoStreamWriter writer, object value, CancellationToken cancellationToken = default)
		{
			return SerializeAsync(writer: writer, value: (T)value, cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Serialize object
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="value">Object to serialize</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public virtual async ValueTask SerializeAsync(ProtoStreamWriter writer, T value, CancellationToken cancellationToken = default)
		{
			foreach(var propertySerializer in this.PropertySerializers)
				await propertySerializer.SerializePropertyAsync(objectInstance: value, writer: writer, cancellationToken: cancellationToken)
					.ConfigureAwait(false);
		}

		/// <summary>
		/// Creates new instance of the object
		/// </summary>
		/// <returns>New object instance</returns>
		public virtual T CreateDeserializingObjectInstance()
		{
			return new T();
		}

		/// <summary>
		/// Deserialize to object
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="previousValue">Previously read object value</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Deserialized object</returns>
		public override async ValueTask<object> DeserializeAsync(ProtoStreamReader reader, object previousValue = default, CancellationToken cancellationToken = default)
		{
			return await DeserializeAsync(reader: reader, previousValue: (T)previousValue, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Deserialize to object
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="previousValue">Previously read object value</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Deserialized object</returns>
		public virtual async ValueTask<T> DeserializeAsync(ProtoStreamReader reader, T previousValue = default, CancellationToken cancellationToken = default)
		{
			FieldHeader fieldHeader;
			PropertySerializer<T> propertySerializer;

			if(previousValue==null)
				previousValue=CreateDeserializingObjectInstance();

			while(true)
			{
				fieldHeader=await reader.ReadFieldHeaderAsync(cancellationToken: cancellationToken)
					.ConfigureAwait(false);

				if(PropertySerializersDictionary.TryGetValue(fieldHeader.FieldNo, out propertySerializer))
				{
					await propertySerializer.DeserializePropertyAsync(objectInstance: previousValue, reader: reader, cancellationToken: cancellationToken)
						.ConfigureAwait(false);
				}
				else if(fieldHeader.FieldNo<=(int)Model.FieldNoEvent.EndGroup)//Group end, leave nested object, end of transmission
					break;
				else if(fieldHeader.FieldLength>0 && fieldHeader.WireType==Internal.WireType.LengthDelimited)//Skip unrecognized length delimited field
				{
					await reader.SkipLengthDelimitedFieldAsync(cancellationToken: cancellationToken)
						.ConfigureAwait(false);
				}
			}

			return previousValue;
		}

		/// <summary>
		/// Creates type map
		/// </summary>
		protected virtual void CreateTypeMap()
		{
			//Dictionary<int, TypeSerializers.TypePropertySerializer<T>> properties = new Dictionary<int, TypeSerializers.TypePropertySerializer<T>>();
			PropertySerializer<T> propertySerializer;
			DataMemberAttribute dataMemberAttribute;
			int fieldNo;

			if(!this.TypeSerializers.ContainsKey(typeof(T)))
			{
				if(null==typeof(T).GetCustomAttribute<DataContractAttribute>())
					throw new NotSupportedException($"Type: {typeof(T).FullName} should have DataMember attribute");

				this.TypeSerializers.Add(typeof(T), this);
				try
				{
					foreach(var propertyInfo in typeof(T).GetProperties(BindingFlags.Instance|BindingFlags.Public))
						if(null!=(dataMemberAttribute=propertyInfo.GetCustomAttribute<DataMemberAttribute>()))
						{
							fieldNo=dataMemberAttribute.Order;

							//Check if Order is set
							if(fieldNo<0)
								throw new InvalidDataContractException($"Order in DataMember attribute has to be set, unique and cannot be negative. Type: {typeof(T).FullName}");

							propertySerializer=(PropertySerializer<T>)Activator.CreateInstance(typeof(PropertySerializer<,>).MakeGenericType(typeof(T), propertyInfo.PropertyType), propertyInfo, new PropertySerializerSettings(fieldNo: fieldNo, serializationType: SerializationType.Default, noPacked: false), this.TypeSerializers);

							try
							{
								this.PropertySerializersDictionary.Add(fieldNo, propertySerializer);
							}
							catch(ArgumentException)
							{
								throw new InvalidDataContractException($"Duplicated order: {fieldNo} in type: {typeof(T).FullName}. Order in DataMember attribute has to be unique for all type's properties.");
							}
							this.PropertySerializers.Add(propertySerializer);
						}
				}
				catch
				{
					this.TypeSerializers.Remove(typeof(T));
					throw;
				}
			}
		}

		/// <summary>
		/// Is <paramref name="value"/> default for the type <typeparamref name="T"/>
		/// </summary>
		/// <param name="value">Value to check</param>
		/// <returns>True if <paramref name="value"/> is default value for type <typeparamref name="T"/>, false otherwise</returns>
		public bool IsDefault(T value)
		{
			return value==null;
		}
	}
}