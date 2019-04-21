using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoStream.PropertySerializers
{
	/// <summary>
	/// Base class for property serializers
	/// </summary>
	public abstract class PropertySerializer
	{
		/// <summary>
		/// Unique field no
		/// </summary>
		public virtual int FieldNo { get; }

		/// <summary>
		/// Property info
		/// </summary>
		public PropertyInfo PropertyInfo { get; }

		/// <summary>
		/// Property has setter, so it can be deserialized
		/// </summary>
		public abstract bool HasSetter { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="propertyInfo">Property info</param>
		/// <param name="propertySettings">Property settings</param>
		protected PropertySerializer(PropertyInfo propertyInfo, PropertySerializerSettings propertySettings)
		{
			this.PropertyInfo=propertyInfo;
			this.FieldNo=propertySettings.FieldNo;
		}
	}

	/// <summary>
	/// Base class for property serializers
	/// </summary>
	/// <typeparam name="T">Type of property's owner object</typeparam>
	public abstract class PropertySerializer<T> : PropertySerializer
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="propertyInfo">Property info</param>
		/// <param name="propertySettings">Property settings</param>
		protected PropertySerializer(PropertyInfo propertyInfo, PropertySerializerSettings propertySettings)
			: base(propertyInfo: propertyInfo, propertySettings: propertySettings)
		{ }

		/// <summary>
		/// Serialize object property
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="objectInstance">Instance of object</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public abstract ValueTask SerializePropertyAsync(ProtoStreamWriter writer, T objectInstance, CancellationToken cancellationToken = default);

		/// <summary>
		/// Deserialize object property
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="objectInstance">Instance of object</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public abstract ValueTask DeserializePropertyAsync(ProtoStreamReader reader, T objectInstance, CancellationToken cancellationToken = default);
	}

	public class PropertySerializer<T, TProperty> : PropertySerializer<T>
	{
		/// <summary>
		/// Property has setter, so it can be deserialized
		/// </summary>
		public override bool HasSetter { get => SetHanlder!=null; }

		/// <summary>
		/// Method for get property value
		/// </summary>
		protected GetHanlder<T, TProperty> GetHanlder { get; }

		/// <summary>
		/// Method for set property value
		/// </summary>
		protected SetHanlder<T, TProperty> SetHanlder { get; }

		/// <summary>
		/// Type serializer
		/// </summary>
		protected ISerializer<TProperty> Serializer { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="propertyInfo">Property info</param>
		/// <param name="propertySettings">Property settings</param>
		/// <param name="typeSerializers">Dictionary of types serializers</param>
		public PropertySerializer(PropertyInfo propertyInfo, PropertySerializerSettings propertySettings, Dictionary<Type, Serializer> typeSerializers)
			: base(propertyInfo: propertyInfo, propertySettings: propertySettings)
		{
			MethodInfo methodInfo;

			Serializer=ProtoStream.Serializer.GetSerializer<TProperty>(serializationType: propertySettings.SerializationType, packed: !propertySettings.NoPacked, typeSerializers: typeSerializers);

			methodInfo=propertyInfo.GetGetMethod();
			if(methodInfo==null)
				throw new InvalidDataContractException($"Property: {propertyInfo.Name} of type: {typeof(T).FullName} should have getter");

			GetHanlder=(GetHanlder<T, TProperty>)methodInfo.CreateDelegate(typeof(GetHanlder<T, TProperty>));

			methodInfo=propertyInfo.GetSetMethod();
			if(methodInfo!=null)
				SetHanlder=(SetHanlder<T, TProperty>)methodInfo.CreateDelegate(typeof(SetHanlder<T, TProperty>));
		}

		/// <summary>
		/// Serialize object property
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="objectInstance">Instance of object</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public override async ValueTask SerializePropertyAsync(ProtoStreamWriter writer, T objectInstance, CancellationToken cancellationToken = default)
		{
			TProperty value;

			value=this.GetHanlder(objectInstance);

			if(!Serializer.IsDefault(value))
				await Serializer.SerializeAsync(writer: writer, fieldNo: FieldNo, value: value, cancellationToken: cancellationToken)
					.ConfigureAwait(false);
		}

		/// <summary>
		/// Deserialize object property
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="objectInstance">Instance of object</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public override async ValueTask DeserializePropertyAsync(ProtoStreamReader reader, T objectInstance, CancellationToken cancellationToken = default)
		{
			TProperty value;

			value=this.GetHanlder(objectInstance);

			try
			{
				value=await Serializer.DeserializeAsync(reader: reader, previousValue: value, cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}
			catch(SerializationException ex)
			{
				throw new SerializationException($"Cannot deserialize field no: {FieldNo} in object: {typeof(T).Name}", ex);
			}

			this.SetHanlder(objectInstance, value);
		}
	}
}