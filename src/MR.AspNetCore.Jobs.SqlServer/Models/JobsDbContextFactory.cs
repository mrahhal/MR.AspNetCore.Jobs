using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace MR.AspNetCore.Jobs.Models
{
	public class JobsDbContextFactory : IDesignTimeDbContextFactory<JobsDbContext>
	{
		public JobsDbContext CreateDbContext(string[] args)
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
