using System;
using MR.AspNetCore.Jobs;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class SqlServerJobsOptionsExtensions
	{
		public static JobsOptions UseSqlServer(this JobsOptions options, string connectionString)
		{
			return options.UseSqlServer(opts =>
			{
				opts.ConnectionString = connectionString;
			});
		}

		public static JobsOptions UseSqlServer(this JobsOptions options, Action<SqlServerOptions> configure)
		{
			if (configure == null) throw new ArgumentNullException(nameof(configure));

			options.RegisterExtension(new SqlServerJobsOptionsExtension(configure));

			return options;
		}
	}
}
