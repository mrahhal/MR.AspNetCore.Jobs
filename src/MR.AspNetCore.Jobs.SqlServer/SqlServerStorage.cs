using System;
using Microsoft.Extensions.Logging;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerStorage : EFCoreStorage<JobsDbContext>
	{
		public SqlServerStorage(
			IServiceProvider provider,
			ILogger<SqlServerStorage> logger)
			: base(provider, logger)
		{
		}
	}
}
