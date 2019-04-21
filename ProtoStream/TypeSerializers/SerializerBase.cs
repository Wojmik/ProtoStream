using ProtoStream.Internal;
using ProtoStream.PropertySerializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Base serializer
	/// </summary>
	public abstract class SerializerBase
	{
		/// <summary>
		/// Get maximum size of item
		/// </summary>
		public virtual int MaxSize { get => int.MaxValue; }
	}

	/// <summary>
	/// Base serializer for type T
	/// </summary>
	/// <typeparam name="T">Type</typeparam>
	public abstract class SerializerBase<T> : SerializerBase, ISerializer<T>
	{
		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="fieldNo">Unique field no</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public abstract ValueTask SerializeAsync(ProtoStreamWriter writer, int fieldNo, T value, CancellationToken cancellationToken = default);

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="previousValue">So far read value</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public abstract ValueTask<T> DeserializeAsync(ProtoStreamReader reader, T previousValue, CancellationToken cancellationToken = default);

		/// <summary>
		/// Is <paramref name="value"/> default for the type <typeparamref name="T"/>
		/// </summary>
		/// <param name="value">Value to check</param>
		/// <returns>True if <paramref name="value"/> is default value for type <typeparamref name="T"/>, false otherwise</returns>
		public abstract bool IsDefault(T value);


		public static SerializerBase<T> CreateSerializer(SerializationType serializationType)
		{
			SerializerBase<T> serializer;
			Type genericType;

			if(typeof(T).IsEnum)//enum
			{
				if(serializationType==SerializationType.FixedSize)
					throw new NotSupportedException("Enum field cannot have FixedSize serialization type");
				else
					switch(Type.GetTypeCode(Enum.GetUnderlyingType(typeof(T))))
					{
						case TypeCode.SByte:
							serializer=(SerializerBase<T>)(SerializerBase)new SByteEnumSerializer<T>();
							break;
						case TypeCode.Byte:
							serializer=(SerializerBase<T>)(SerializerBase)new ByteEnumSerializer<T>();
							break;
						case TypeCode.Int16:
							serializer=(SerializerBase<T>)(SerializerBase)new ShortEnumSerializer<T>();
							break;
						case TypeCode.UInt16:
							serializer=(SerializerBase<T>)(SerializerBase)new UShortEnumSerializer<T>();
							break;
						case TypeCode.Int32:
							serializer=(SerializerBase<T>)(SerializerBase)new IntEnumSerializer<T>();
							break;
						case TypeCode.UInt32:
							serializer=(SerializerBase<T>)(SerializerBase)new UIntEnumSerializer<T>();
							break;
						case TypeCode.Int64:
							serializer=(SerializerBase<T>)(SerializerBase)new LongEnumSerializer<T>();
							break;
						case TypeCode.UInt64:
							serializer=(SerializerBase<T>)(SerializerBase)new ULongEnumSerializer<T>();
							break;
						default:
							throw new NotSupportedException($"Unsupported enum underlying type: {Enum.GetUnderlyingType(typeof(T))} for enum: {typeof(T)}");
					}
			}
			else
				switch(Type.GetTypeCode(typeof(T)))
				{
					case TypeCode.Boolean:
						if(serializationType==SerializationType.FixedSize)
							throw new NotSupportedException("Bool field cannot have FixedSize serialization type");
						else
							serializer=(SerializerBase<T>)(SerializerBase)BoolSerializer.Default;
						break;
					case TypeCode.Char:
						if(serializationType==SerializationType.FixedSize)
							throw new NotSupportedException("Char field cannot have FixedSize serialization type");
						else
							serializer=(SerializerBase<T>)(SerializerBase)VarCharSerializer.Default;
						break;
					case TypeCode.SByte:
						if(serializationType==SerializationType.FixedSize)
							throw new NotSupportedException("SByte field cannot have FixedSize serialization type");
						else
							serializer=(SerializerBase<T>)(SerializerBase)VarSByteSerializer.Default;
						break;
					case TypeCode.Byte:
						if(serializationType==SerializationType.FixedSize)
							throw new NotSupportedException("Byte field cannot have FixedSize serialization type");
						else
							serializer=(SerializerBase<T>)(SerializerBase)VarByteSerializer.Default;
						break;
					case TypeCode.Int16:
						if(serializationType==SerializationType.FixedSize)
							throw new NotSupportedException("Int16 field cannot have FixedSize serialization type");
						else
							serializer=(SerializerBase<T>)(SerializerBase)VarShortSerializer.Default;
						break;
					case TypeCode.UInt16:
						if(serializationType==SerializationType.FixedSize)
							throw new NotSupportedException("UInt16 field cannot have FixedSize serialization type");
						else
							serializer=(SerializerBase<T>)(SerializerBase)VarUShortSerializer.Default;
						break;
					case TypeCode.Int32:
						if(serializationType==SerializationType.FixedSize)
							serializer=(SerializerBase<T>)(SerializerBase)FixedIntSerializer.Default;
						else
							serializer=(SerializerBase<T>)(SerializerBase)VarIntSerializer.Default;
						break;
					case TypeCode.UInt32:
						if(serializationType==SerializationType.FixedSize)
							serializer=(SerializerBase<T>)(SerializerBase)FixedUIntSerializer.Default;
						else
							serializer=(SerializerBase<T>)(SerializerBase)VarUIntSerializer.Default;
						break;
					case TypeCode.Int64:
						if(serializationType==SerializationType.FixedSize)
							serializer=(SerializerBase<T>)(SerializerBase)FixedLongSerializer.Default;
						else
							serializer=(SerializerBase<T>)(SerializerBase)VarLongSerializer.Default;
						break;
					case TypeCode.UInt64:
						if(serializationType==SerializationType.FixedSize)
							serializer=(SerializerBase<T>)(SerializerBase)FixedULongSerializer.Default;
						else
							serializer=(SerializerBase<T>)(SerializerBase)VarULongSerializer.Default;
						break;
					case TypeCode.Single:
						if(serializationType==SerializationType.VarInt)
							throw new NotSupportedException("Single field cannot have VarInt serialization type");
						else
							serializer=(SerializerBase<T>)(SerializerBase)FloatSerializer.Default;
						break;
					case TypeCode.Double:
						if(serializationType==SerializationType.VarInt)
							throw new NotSupportedException("Double field cannot have VarInt serialization type");
						else
							serializer=(SerializerBase<T>)(SerializerBase)DoubleSerializer.Default;
						break;
					case TypeCode.Decimal:
						if(serializationType!=SerializationType.Default)
							throw new NotSupportedException("Decimal field can only have Default serialization type");
						else
							serializer=(SerializerBase<T>)(SerializerBase)DecimalSerializer.Default;
						break;
					case TypeCode.DateTime:
						if(serializationType==SerializationType.VarInt)
							serializer=(SerializerBase<T>)(SerializerBase)VarDateTimeSerializer.Default;
						else
							serializer=(SerializerBase<T>)(SerializerBase)FixedDateTimeSerializer.Default;
						break;
					case TypeCode.String:
						if(serializationType!=SerializationType.Default)
							throw new NotSupportedException("String field can only have Default serialization type");
						else
							serializer=(SerializerBase<T>)(SerializerBase)StringSerializer.Default;
						break;
					default:
						if(typeof(T)==typeof(TimeSpan))
							if(serializationType==SerializationType.FixedSize)
								serializer=(SerializerBase<T>)(SerializerBase)FixedTimeSpanSerializer.Default;
							else
								serializer=(SerializerBase<T>)(SerializerBase)VarTimeSpanSerializer.Default;
						else if(typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition()==typeof(Nullable<>))//Nullable<>
						{
							genericType=typeof(T).GenericTypeArguments[0];
							if(genericType.IsEnum)//enum?
							{
								if(serializationType==SerializationType.FixedSize)
									throw new NotSupportedException("Enum? field cannot have FixedSize serialization type");
								else
									switch(Type.GetTypeCode(Enum.GetUnderlyingType(genericType)))
									{
										case TypeCode.SByte:
											serializer=(SerializerBase<T>)Activator.CreateInstance(typeof(NullableSerializer<>).MakeGenericType(genericType), Activator.CreateInstance(typeof(SByteEnumSerializer<>).MakeGenericType(genericType)));
											break;
										case TypeCode.Byte:
											serializer=(SerializerBase<T>)Activator.CreateInstance(typeof(NullableSerializer<>).MakeGenericType(genericType), Activator.CreateInstance(typeof(ByteEnumSerializer<>).MakeGenericType(genericType)));
											break;
										case TypeCode.Int16:
											serializer=(SerializerBase<T>)Activator.CreateInstance(typeof(NullableSerializer<>).MakeGenericType(genericType), Activator.CreateInstance(typeof(ShortEnumSerializer<>).MakeGenericType(genericType)));
											break;
										case TypeCode.UInt16:
											serializer=(SerializerBase<T>)Activator.CreateInstance(typeof(NullableSerializer<>).MakeGenericType(genericType), Activator.CreateInstance(typeof(UShortEnumSerializer<>).MakeGenericType(genericType)));
											break;
										case TypeCode.Int32:
											serializer=(SerializerBase<T>)Activator.CreateInstance(typeof(NullableSerializer<>).MakeGenericType(genericType), Activator.CreateInstance(typeof(IntEnumSerializer<>).MakeGenericType(genericType)));
											break;
										case TypeCode.UInt32:
											serializer=(SerializerBase<T>)Activator.CreateInstance(typeof(NullableSerializer<>).MakeGenericType(genericType), Activator.CreateInstance(typeof(UIntEnumSerializer<>).MakeGenericType(genericType)));
											break;
										case TypeCode.Int64:
											serializer=(SerializerBase<T>)Activator.CreateInstance(typeof(NullableSerializer<>).MakeGenericType(genericType), Activator.CreateInstance(typeof(LongEnumSerializer<>).MakeGenericType(genericType)));
											break;
										case TypeCode.UInt64:
											serializer=(SerializerBase<T>)Activator.CreateInstance(typeof(NullableSerializer<>).MakeGenericType(genericType), Activator.CreateInstance(typeof(ULongEnumSerializer<>).MakeGenericType(genericType)));
											break;
										default:
											throw new NotSupportedException($"Unsupported enum? underlying type: {Enum.GetUnderlyingType(genericType)} for enum?: {genericType}");
									}
							}
							else
								switch(Type.GetTypeCode(genericType))
								{
									case TypeCode.Boolean:
										if(serializationType==SerializationType.FixedSize)
											throw new NotSupportedException("Bool field cannot have FixedSize serialization type");
										else
											serializer=(SerializerBase<T>)(SerializerBase)BoolNullableSerializer.Default;
										break;
									case TypeCode.Char:
										if(serializationType==SerializationType.FixedSize)
											throw new NotSupportedException("Char field cannot have FixedSize serialization type");
										else
											serializer=(SerializerBase<T>)(SerializerBase)VarCharNullableSerializer.Default;
										break;
									case TypeCode.SByte:
										if(serializationType==SerializationType.FixedSize)
											throw new NotSupportedException("SByte field cannot have FixedSize serialization type");
										else
											serializer=(SerializerBase<T>)(SerializerBase)VarSByteNullableSerializer.Default;
										break;
									case TypeCode.Byte:
										if(serializationType==SerializationType.FixedSize)
											throw new NotSupportedException("Byte field cannot have FixedSize serialization type");
										else
											serializer=(SerializerBase<T>)(SerializerBase)VarByteNullableSerializer.Default;
										break;
									case TypeCode.Int16:
										if(serializationType==SerializationType.FixedSize)
											throw new NotSupportedException("Int16 field cannot have FixedSize serialization type");
										else
											serializer=(SerializerBase<T>)(SerializerBase)VarShortNullableSerializer.Default;
										break;
									case TypeCode.UInt16:
										if(serializationType==SerializationType.FixedSize)
											throw new NotSupportedException("UInt16 field cannot have FixedSize serialization type");
										else
											serializer=(SerializerBase<T>)(SerializerBase)VarUShortNullableSerializer.Default;
										break;
									case TypeCode.Int32:
										if(serializationType==SerializationType.FixedSize)
											serializer=(SerializerBase<T>)(SerializerBase)FixedIntNullableSerializer.Default;
										else
											serializer=(SerializerBase<T>)(SerializerBase)VarIntNullableSerializer.Default;
										break;
									case TypeCode.UInt32:
										if(serializationType==SerializationType.FixedSize)
											serializer=(SerializerBase<T>)(SerializerBase)FixedUIntNullableSerializer.Default;
										else
											serializer=(SerializerBase<T>)(SerializerBase)VarUIntNullableSerializer.Default;
										break;
									case TypeCode.Int64:
										if(serializationType==SerializationType.FixedSize)
											serializer=(SerializerBase<T>)(SerializerBase)FixedLongNullableSerializer.Default;
										else
											serializer=(SerializerBase<T>)(SerializerBase)VarLongNullableSerializer.Default;
										break;
									case TypeCode.UInt64:
										if(serializationType==SerializationType.FixedSize)
											serializer=(SerializerBase<T>)(SerializerBase)FixedULongNullableSerializer.Default;
										else
											serializer=(SerializerBase<T>)(SerializerBase)VarULongNullableSerializer.Default;
										break;
									case TypeCode.Single:
										if(serializationType==SerializationType.VarInt)
											throw new NotSupportedException("Single field cannot have VarInt serialization type");
										else
											serializer=(SerializerBase<T>)(SerializerBase)FloatNullableSerializer.Default;
										break;
									case TypeCode.Double:
										if(serializationType==SerializationType.VarInt)
											throw new NotSupportedException("Double field cannot have VarInt serialization type");
										else
											serializer=(SerializerBase<T>)(SerializerBase)DoubleNullableSerializer.Default;
										break;
									case TypeCode.Decimal:
										if(serializationType!=SerializationType.Default)
											throw new NotSupportedException("Decimal field can only have Default serialization type");
										else
											serializer=(SerializerBase<T>)(SerializerBase)DecimalNullableSerializer.Default;
										break;
									case TypeCode.DateTime:
										if(serializationType==SerializationType.VarInt)
											serializer=(SerializerBase<T>)(SerializerBase)VarDateTimeNullableSerializer.Default;
										else
											serializer=(SerializerBase<T>)(SerializerBase)FixedDateTimeNullableSerializer.Default;
										break;
									default:
										if(genericType==typeof(TimeSpan))
											if(serializationType==SerializationType.FixedSize)
												serializer=(SerializerBase<T>)(SerializerBase)FixedTimeSpanNullableSerializer.Default;
											else
												serializer=(SerializerBase<T>)(SerializerBase)VarTimeSpanNullableSerializer.Default;
										else
											throw new NotSupportedException($"Unsupported Nullable type: {typeof(T)}");
										break;
								}
						}
						else if(typeof(T)==typeof(byte[]))
							if(serializationType!=SerializationType.Default)
								throw new NotSupportedException("Byte array field can only have Default serialization type");
							else
								serializer=(SerializerBase<T>)(SerializerBase)ByteArraySerializer.Default;
						else
							serializer=null;
						break;
				}

			return serializer;
		}
	}
}