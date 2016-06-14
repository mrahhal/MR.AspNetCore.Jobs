using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Dapper;
using Microsoft.Extensions.Logging;

namespace MR.AspNetCore.Jobs
{
	public static class SqlServerObjectsInstaller
	{
		private const int RequiredSchemaVersion = 1;
		private const int RetryAttempts = 3;

		public static void Install(SqlConnection connection, ILogger logger)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			logger?.LogInformation("Installing Jobs SQL objects...");

			var script = GetStringResource(
				typeof(SqlServerObjectsInstaller).GetTypeInfo().Assembly,
				"MR.AspNetCore.Jobs.SqlServer.install.sql");

			script = script.Replace("SET @TARGET_SCHEMA_VERSION = 0;", "SET @TARGET_SCHEMA_VERSION = " + RequiredSchemaVersion + ";");

			for (var i = 0; i < RetryAttempts; i++)
			{
				try
				{
					connection.Execute(script);
					break;
				}
				catch (SqlException ex)
				{
					if (i == RetryAttempts - 1)
					{
						throw;
					}
					logger?.LogWarning("Exception occurred during automatic migration. Retrying...", ex);
				}
			}

			logger?.LogInformation("Jobs SQL objects installed.");
		}

		private static string GetStringResource(Assembly assembly, string resourceName)
		{
			using (var stream = assembly.GetManifestResourceStream(resourceName))
			{
				if (stream == null)
				{
					throw new InvalidOperationException(String.Format(
						"Requested resource `{0}` was not found in the assembly `{1}`.",
						resourceName,
						assembly));
				}

				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}
