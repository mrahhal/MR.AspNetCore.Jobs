using System;
using Microsoft.Extensions.Logging;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs.Server
{
	public class SqlServerExpirationManager : EFCoreExpirationManager<JobsDbContext>
	{
		public SqlServerExpirationManager(
			ILogger<SqlServerExpirationManager> logger,
			SqlServerOptions options,
			IServiceProvider provider)
			: base(logger, options, provider)
		{
		}

		protected override string CreateDeleteTopQuery(string schema, string table)
		{
			return $@"
DELETE TOP (@count)
FROM [{schema}].[{table}] WITH (readpast)
WHERE ExpiresAt < @now;";
		}
	}
}
