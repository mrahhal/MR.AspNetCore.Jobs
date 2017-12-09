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

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.HasDefaultSchema(RelationalOptions.Schema);

			builder.Entity<Job>(b =>
			{
				b.Property(x => x.StateName).IsRequired();

				b.HasIndex(x => new { x.Due, x.StateName });
				b.HasIndex(x => x.StateName);
				b.HasIndex(x => x.Added);
				b.HasIndex(x => x.Updated);
			});

			builder.Entity<CronJob>(b =>
			{
				b.Property(x => x.Name).IsRequired();

				b.HasIndex(x => x.Name).IsUnique();
			});

			builder.Entity<JobQueue>(b =>
			{
				b.HasIndex(x => x.FetchedAt);
			});
		}
	}
}
