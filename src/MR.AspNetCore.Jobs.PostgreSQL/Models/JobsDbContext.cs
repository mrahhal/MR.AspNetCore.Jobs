using Microsoft.EntityFrameworkCore;

namespace MR.AspNetCore.Jobs.Models
{
	public class JobsDbContext : EFCoreJobsDbContext
	{
		private PostgreSQLOptions _PostgreSQLOptions;

		public JobsDbContext()
		{
		}

		public JobsDbContext(
			DbContextOptions<JobsDbContext> options,
			PostgreSQLOptions PostgreSQLOptions)
			: base(options, PostgreSQLOptions)
		{
			_PostgreSQLOptions = PostgreSQLOptions;
		}
	}
}
