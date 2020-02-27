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
	/// Serializer for double fields
	/// </summary>
	public class DoubleSerializer : SerializerValueBase<double>
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static DoubleSerializer Default { get; } = new DoubleSerializer();

		/// <summary>
		/// Get maximum size of item
		/// </summary>
		public override int MaxSize { get => sizeof(long); }

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="fieldNo">Unique field no</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public override async ValueTask SerializeAsync(ProtoStreamWriter writer, int fieldNo, double value, CancellationToken cancellationToken = default)
		{
			await writer.WriteInt64Async(fieldNo: fieldNo, value: DoubleToInt64Bits(value), cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="previousValue">So far read value</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public override async ValueTask<double> DeserializeAsync(ProtoStreamReader reader, double previousValue, CancellationToken cancellationToken = default)
		{
			WireType wireType;
			double value;

			wireType=reader.CurrentFieldHeader.WireType;
			if(wireType==Internal.WireType.Fixed64)
				value=Int64BitsToDouble(await reader.ReadInt64Async(cancellationToken: cancellationToken)
					.ConfigureAwait(false));
			else if(wireType==Internal.WireType.Fixed32)
				value=Int32BitsToSingle(await reader.ReadInt32Async(cancellationToken: cancellationToken)
					.ConfigureAwait(false));
			else
				throw new SerializationException($"Unexpected wire type: {wireType} for type: {typeof(double)}");

			return value;
		}

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public override async ValueTask SerializeValueAsync(ProtoStreamWriter writer, double value, CancellationToken cancellationToken = default)
		{
			await writer.WriteInt64ValueAsync(value: DoubleToInt64Bits(value), cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public override async ValueTask<ValueWithSize<double>> DeserializeValueAsync(ProtoStreamReader reader, CancellationToken cancellationToken = default)
		{
			return new ValueWithSize<double>(value: Int64BitsToDouble(await reader.ReadInt64Async(cancellationToken: cancellationToken).ConfigureAwait(false)), size: sizeof(long));
		}

		/// <summary>
		/// Is <paramref name="value"/> default value
		/// </summary>
		/// <param name="value">Value to check</param>
		/// <returns>True if <paramref name="value"/> is default value, false otherwise</returns>
		public override bool IsDefault(double value)
		{
			return value==default;
		}

#if !NETCOREAPP
		protected float Int32BitsToSingle(int value)
		{
			Span<int> buf = stackalloc int[] { value, };
			
			return System.Runtime.InteropServices.MemoryMarshal.Cast<int, float>(buf)[0];
		}
#endif
	}
}