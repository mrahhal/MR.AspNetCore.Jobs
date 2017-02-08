using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace MR.AspNetCore.Jobs.Models
{
	public class JobsDbContext : DbContext
	{
		internal static string DevConnectionString =
			@"Server=.\sqlexpress;Database=MR.AspNetCore.Jobs.Dev;Trusted_Connection=True;";

		private SqlServerOptions _sqlServerOptions;

		public JobsDbContext()
		{
		}

		public JobsDbContext(DbContextOptions<JobsDbContext> options, SqlServerOptions sqlServerOptions)
			: base(options)
		{
			_sqlServerOptions = sqlServerOptions;
		}

		public DbSet<CronJob> CronJobs { get; set; }

		public DbSet<Job> Jobs { get; set; }

		public DbSet<JobQueue> JobQueue { get; set; }

		public DbConnection GetDbConnection() => Database.GetDbConnection();

		protected override void OnConfiguring(DbContextOptionsBuilder builder)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.HasDefaultSchema(_sqlServerOptions.Schema);

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
