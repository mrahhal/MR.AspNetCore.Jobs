using System;
using MR.AspNetCore.Jobs;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class PostgreSQLJobsOptionsExtensions
	{
		public static JobsOptions UsePostgreSQL(this JobsOptions options, string connectionString)
		{
			return options.UsePostgreSQL(opts =>
			{
				opts.ConnectionString = connectionString;
			});
		}

		public static JobsOptions UsePostgreSQL(this JobsOptions options, Action<PostgreSQLOptions> configure)
		{
			if (configure == null) throw new ArgumentNullException(nameof(configure));

			options.RegisterExtension(new PostgreSQLJobsOptionsExtension(configure));

			return options;
		}
	}
}
