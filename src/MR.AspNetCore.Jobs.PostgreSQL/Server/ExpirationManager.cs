using System;
using Microsoft.Extensions.Logging;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs.Server
{
	public class PostgreSQLExpirationManager : EFCoreExpirationManager<JobsDbContext>
	{
		public PostgreSQLExpirationManager(
			ILogger<PostgreSQLExpirationManager> logger,
			PostgreSQLOptions options,
			IServiceProvider provider)
			: base(logger, options, provider)
		{
		}

		protected override string CreateDeleteTopQuery(string schema, string table)
		{
			return $@"
DELETE
FROM ""{schema}"".""{table}""
WHERE ExpiresAt < @now
LIMIT @count";
		}
	}
}
