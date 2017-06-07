using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MR.AspNetCore.Jobs.Models
{
	public class JobsDbContextFactory : IDbContextFactory<JobsDbContext>
	{
		public JobsDbContext Create(DbContextFactoryOptions options)
		{
			var services = new ServiceCollection();

			services.AddSingleton(new SqlServerOptions());
			services.AddDbContext<JobsDbContext>(opts =>
			{
				opts.UseSqlServer(JobsDbContext.DevConnectionString, sqlOpts =>
				{
					sqlOpts.MigrationsHistoryTable(
						SqlServerOptions.DefaultMigrationsHistoryTableName,
						SqlServerOptions.DefaultSchema);
				});
			});

			return services.BuildServiceProvider().GetRequiredService<JobsDbContext>();
		}
	}
}
