using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoStreamFormatter
{
	public class ProtoStreamFormatter : OutputFormatter
	{
		Dictionary<Type, SerializerData> SupportedTypes { get; }

		public ProtoStreamFormatter(IEnumerable<Type> supportedTypes)
		{
			Dictionary<Type, ProtoStream.Serializer> typesDictionary = new Dictionary<Type, ProtoStream.Serializer>();
			Type[] parameterTypes = new Type[] { typeof(ProtoStream.PropertySerializers.SerializationType), typeof(bool), typeof(Dictionary<Type, ProtoStream.Serializer>), };
			object[] parameters = new object[] { ProtoStream.PropertySerializers.SerializationType.Default, true, typesDictionary, };

			SupportedMediaTypes.Add("application/protostream");

			foreach(var type in supportedTypes)
			{
				typeof(ProtoStream.Serializer)
					.GetMethod(nameof(ProtoStream.Serializer.GetSerializer), parameterTypes)
					.MakeGenericMethod(type)
					.Invoke(null, parameters);
			}

			SupportedTypes=typesDictionary
				.ToDictionary(keyVal => keyVal.Key, keyVal =>
				{
					SerializerData serializerData;
					SerializeAsyncHandler dlgt;

					dlgt=(SerializeAsyncHandler)typeof(ProtoStream.Serializer)
						.GetMethod(nameof(ProtoStream.Serializer<object>.SerializeAsync), new Type[] { typeof(ProtoStream.ProtoStreamWriter), typeof(object), typeof(CancellationToken), })
						.CreateDelegate(typeof(SerializeAsyncHandler));

					serializerData=new SerializerData() { Serializer=keyVal.Value, SerializeAsyncMethod=dlgt };

					return serializerData;
				});
		}

		protected override bool CanWriteType(Type type)
		{
			if(SupportedTypes.ContainsKey(type))
				return base.CanWriteType(type);
			else
				return false;
		}

		public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
		{
			SerializerData serializerData;

			if(SupportedTypes.TryGetValue(context.ObjectType, out serializerData))
			{
				using(var writer = new ProtoStream.ProtoStreamWriter(stream: context.HttpContext.Response.Body))
				{
					await serializerData.SerializeAsyncMethod.Invoke(serializer: serializerData.Serializer, writer: writer, value: context.Object, cancellationToken: default);
				}
			}
			else
				throw new NotSupportedException($"ProtoStream formatter not support type: {context.ObjectType}");
		}
	}
}