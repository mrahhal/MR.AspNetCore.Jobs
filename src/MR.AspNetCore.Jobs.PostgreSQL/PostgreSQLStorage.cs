using System;
using Microsoft.Extensions.Logging;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs
{
	public class PostgreSQLStorage : EFCoreStorage<JobsDbContext>
	{
		public PostgreSQLStorage(
			IServiceProvider provider,
			ILogger<PostgreSQLStorage> logger)
			: base(provider, logger)
		{
		}
	}
}
