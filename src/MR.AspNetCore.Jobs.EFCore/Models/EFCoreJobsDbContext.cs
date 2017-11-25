using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace MR.AspNetCore.Jobs.Models
{
	public abstract class EFCoreJobsDbContext : DbContext
	{
		private EFCoreOptions _relationalOptions;

		public EFCoreJobsDbContext()
		{
		}

		public EFCoreJobsDbContext(
			DbContextOptions options,
			EFCoreOptions relationalOptions)
			: base(options)
		{
			_relationalOptions = relationalOptions;
		}

		public DbSet<CronJob> CronJobs { get; set; }

		public DbSet<Job> Jobs { get; set; }

		public DbSet<JobQueue> JobQueue { get; set; }

		public DbConnection GetDbConnection() => Database.GetDbConnection();

		protected override void OnConfiguring(DbContextOptionsBuilder builder)
		{
			base.OnConfiguring(builder);
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.HasDefaultSchema(_relationalOptions.Schema);

			builder.Entity<Job>(b =>
			{
				b.Property(x => x.StateName).IsRequired();

				b.HasIndex(x => new { x.Due, x.StateName });
				b.HasIndex(x => x.StateName);
			});

			builder.Entity<CronJob>(b =>
			{
				b.Property(x => x.Name).IsRequired();

				b.HasIndex(x => x.Name).IsUnique();
			});
		}
	}
}
