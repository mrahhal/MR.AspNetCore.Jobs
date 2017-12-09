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

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.HasDefaultSchema(RelationalOptions.Schema);

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

			builder.Entity<JobQueue>(b =>
			{
				b.Ignore(x => x.FetchedAt);
			});
		}
	}
}
