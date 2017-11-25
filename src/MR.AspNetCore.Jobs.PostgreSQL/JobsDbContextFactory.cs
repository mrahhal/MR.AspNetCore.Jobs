using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs
{
	public class JobsDbContextFactory : IDesignTimeDbContextFactory<JobsDbContext>
	{
		internal static string DevConnectionString =
			Environment.GetEnvironmentVariable("MR_ASPNETCORE_JOBS_POSTGRESQL_CS_DEV") ??
			@"Server=127.0.0.1;Port=5432;Database=MR.AspNetCore.Jobs.Dev;User Id=postgres;Password=password;";

		public JobsDbContext CreateDbContext(string[] args)
		{
			var services = new ServiceCollection();

			services.AddSingleton(new PostgreSQLOptions());
			services.AddDbContext<JobsDbContext>(opts =>
			{
				opts.UseNpgsql(DevConnectionString, sqlOpts =>
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
