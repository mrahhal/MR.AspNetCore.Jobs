using System;
using Microsoft.Extensions.Logging;

namespace MR.AspNetCore.Jobs
{
	internal static class LoggerExtensions
	{
		private static Action<ILogger, Exception> _collectingExpiredEntities = LoggerMessage.Define(
			LogLevel.Debug,
			1,
			"Collecting expired entities.");

		public static void CollectingExpiredEntities(this ILogger logger)
		{
			_collectingExpiredEntities(logger, null);
		}
	}
}
