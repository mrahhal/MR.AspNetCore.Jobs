using System;
using Microsoft.Extensions.Logging;

namespace MR.AspNetCore.Jobs
{
	internal static class JobsSqlServerLoggerExtensions
	{
		private static Action<ILogger, Exception> _collectingExpiredEntities;

		private static Action<ILogger, Exception> _installing;
		private static Action<ILogger, Exception> _installingError;
		private static Action<ILogger, Exception> _installingSuccess;

		static JobsSqlServerLoggerExtensions()
		{
			_collectingExpiredEntities = LoggerMessage.Define(
				LogLevel.Debug,
				1,
				"Collecting expired entities");

			_installing = LoggerMessage.Define(
				LogLevel.Debug,
				1,
				"Installing Jobs SQL objects...");

			_installingError = LoggerMessage.Define(
				LogLevel.Warning,
				2,
				"Exception occurred during automatic migration. Retrying...");

			_installingSuccess = LoggerMessage.Define(
				LogLevel.Debug,
				3,
				"Jobs SQL objects installed.");
		}

		public static void CollectingExpiredEntities(this ILogger logger)
		{
			_collectingExpiredEntities(logger, null);
		}

		public static void Installing(this ILogger logger)
		{
			_installing(logger, null);
		}

		public static void InstallingError(this ILogger logger, Exception exception)
		{
			_installingError(logger, exception);
		}

		public static void InstallingSuccess(this ILogger logger)
		{
			_installingSuccess(logger, null);
		}
	}
}
