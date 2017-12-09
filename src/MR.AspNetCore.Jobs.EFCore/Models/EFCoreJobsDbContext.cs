using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace MR.AspNetCore.Jobs.Models
{
	public abstract class EFCoreJobsDbContext : DbContext
	{
		public EFCoreJobsDbContext()
		{
		}

		public EFCoreJobsDbContext(
			DbContextOptions options,
			EFCoreOptions relationalOptions)
			: base(options)
		{
			RelationalOptions = relationalOptions;
		}

		public EFCoreOptions RelationalOptions { get; }

		public DbConnection GetDbConnection() => Database.GetDbConnection();

		public DbSet<CronJob> CronJobs { get; set; }

		public DbSet<Job> Jobs { get; set; }

		public DbSet<JobQueue> JobQueue { get; set; }
	}
}
