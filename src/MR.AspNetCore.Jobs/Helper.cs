using System;
using Newtonsoft.Json;

namespace MR.AspNetCore.Jobs
{
	internal static class Helper
	{
		private static JsonSerializerSettings _serializerSettings;

		public static void SetSerializerSettings(JsonSerializerSettings setting)
		{
			_serializerSettings = setting;
		}

		public static string ToJson(object value)
		{
			return value != null
				? JsonConvert.SerializeObject(value, _serializerSettings)
				: null;
		}

		public static T FromJson<T>(string value)
		{
			return value != null
				? JsonConvert.DeserializeObject<T>(value, _serializerSettings)
				: default(T);
		}

		public static object FromJson(string value, Type type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			return value != null
				? JsonConvert.DeserializeObject(value, type, _serializerSettings)
				: null;
		}

		private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static long ToTimestamp(DateTime value)
		{
			TimeSpan elapsedTime = value - Epoch;
			return (long)elapsedTime.TotalSeconds;
		}

		public static DateTime FromTimestamp(long value)
		{
			return Epoch.AddSeconds(value);
		}
	}
}
