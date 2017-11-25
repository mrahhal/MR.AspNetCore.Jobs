using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs
{
	public class JobsDbContextFactory : IDesignTimeDbContextFactory<JobsDbContext>
	{
		internal static string DevConnectionString =
			@"Server=.\sqlexpress;Database=MR.AspNetCore.Jobs.Dev;Trusted_Connection=True;";

		public JobsDbContext CreateDbContext(string[] args)
		{
			var services = new ServiceCollection();

			services.AddSingleton(new SqlServerOptions());
			services.AddDbContext<JobsDbContext>(opts =>
			{
				opts.UseSqlServer(DevConnectionString, sqlOpts =>
				{
					sqlOpts.MigrationsHistoryTable(
						EFCoreOptions.DefaultMigrationsHistoryTableName,
						EFCoreOptions.DefaultSchema);
				});
			});

			return services.BuildServiceProvider().GetRequiredService<JobsDbContext>();
		}
	}
}
