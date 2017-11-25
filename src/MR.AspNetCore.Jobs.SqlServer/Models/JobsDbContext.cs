using Microsoft.EntityFrameworkCore;

namespace MR.AspNetCore.Jobs.Models
{
	public class JobsDbContext : EFCoreJobsDbContext
	{
		private SqlServerOptions _sqlServerOptions;

		public JobsDbContext()
		{
		}

		public JobsDbContext(
			DbContextOptions<JobsDbContext> options,
			SqlServerOptions sqlServerOptions)
			: base(options, sqlServerOptions)
		{
			_sqlServerOptions = sqlServerOptions;
		}
	}
}
