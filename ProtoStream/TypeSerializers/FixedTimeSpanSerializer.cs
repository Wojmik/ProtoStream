﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoStream.TypeSerializers
{
	/// <summary>
	/// Serializer for TimeSpan fields serialized as Fixed
	/// </summary>
	public class FixedTimeSpanSerializer : TimeSpanSerializerBase
	{
		/// <summary>
		/// Default instance of serializer
		/// </summary>
		public static FixedTimeSpanSerializer Default { get; } = new FixedTimeSpanSerializer();

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
		public override async ValueTask SerializeAsync(ProtoStreamWriter writer, int fieldNo, TimeSpan value, CancellationToken cancellationToken = default)
		{
			await writer.WriteInt64Async(fieldNo: fieldNo, value: value.Ticks, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Serialize value
		/// </summary>
		/// <param name="writer">ProtoStream writer</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns></returns>
		public override async ValueTask SerializeValueAsync(ProtoStreamWriter writer, TimeSpan value, CancellationToken cancellationToken = default)
		{
			await writer.WriteInt64ValueAsync(value: value.Ticks, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Deserialize value
		/// </summary>
		/// <param name="reader">ProtoStream reader</param>
		/// <param name="cancellationToken">Cancallation token</param>
		/// <returns>Deserialized value</returns>
		public override async ValueTask<ValueWithSize<TimeSpan>> DeserializeValueAsync(ProtoStreamReader reader, CancellationToken cancellationToken = default)
		{
			ValueWithSize<TimeSpan> value;

			value.Value=new TimeSpan(await reader.ReadInt64Async(cancellationToken: cancellationToken)
				.ConfigureAwait(false));
			value.Size=sizeof(long);

			return value;
		}
	}
}