using ProtoStream.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.BitConverter;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for float fields
	/// </summary>
	public class FloatSerializer : SerializerValueBase<float>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static FloatSerializer Default { get; } = new FloatSerializer();

		/// <summary>
		/// Get maximum size of item
		/// </summary>
		public override int MaxSize { get => sizeof(int); }

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="fieldNo">Unique field no</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public override async ValueTask SerializeAsync(ProtoStreamWriter writer, int fieldNo, float value, CancellationToken cancellationToken = default)
		{
			await writer.WriteInt32Async(fieldNo: fieldNo, value: SingleToInt32Bits(value), cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="previousValue">So far read value</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public override async ValueTask<float> DeserializeAsync(ProtoStreamReader reader, float previousValue, CancellationToken cancellationToken = default)
		{
			WireType wireType;
			float value;

			wireType=reader.CurrentFieldHeader.WireType;
			if(wireType==Internal.WireType.Fixed32)
				value=Int32BitsToSingle(await reader.ReadInt32Async(cancellationToken: cancellationToken)
					.ConfigureAwait(false));
			else if(wireType==Internal.WireType.Fixed64)
				value=(float)Int64BitsToDouble(await reader.ReadInt64Async(cancellationToken: cancellationToken)
					.ConfigureAwait(false));
			else
				throw new SerializationException($"Unexpected wire type: {wireType} for type: {typeof(float)}");

			return value;
		}

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public override async ValueTask SerializeValueAsync(ProtoStreamWriter writer, float value, CancellationToken cancellationToken = default)
		{
			await writer.WriteInt32ValueAsync(value: SingleToInt32Bits(value), cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public override async ValueTask<ValueWithSize<float>> DeserializeValueAsync(ProtoStreamReader reader, CancellationToken cancellationToken = default)
		{
			return new ValueWithSize<float>(value: Int32BitsToSingle(await reader.ReadInt32Async(cancellationToken: cancellationToken).ConfigureAwait(false)), size: sizeof(int));
		}

		/// <summary>
		/// Is <paramref name="value"/> default value
		/// </summary>
		/// <param name="value">Value to check</param>
		/// <returns>True if <paramref name="value"/> is default value, false otherwise</returns>
		public override bool IsDefault(float value)
		{
			return value==default;
		}

#if !NETCOREAPP
		protected int SingleToInt32Bits(float value)
		{
			Span<float> buf = stackalloc float[] { value, };

			return System.Runtime.InteropServices.MemoryMarshal.Cast<float, int>(buf)[0];
		}

		protected float Int32BitsToSingle(int value)
		{
			Span<int> buf = stackalloc int[] { value, };
			
			return System.Runtime.InteropServices.MemoryMarshal.Cast<int, float>(buf)[0];
		}
#endif
	}
}