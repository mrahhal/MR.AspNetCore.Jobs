using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server;

namespace MR.AspNetCore.Jobs
{
	public class PostgreSQLJobsOptionsExtension : IJobsOptionsExtension
	{
		private Action<PostgreSQLOptions> _configure;

		public PostgreSQLJobsOptionsExtension(Action<PostgreSQLOptions> configure)
		{
			_configure = configure;
		}

		public void AddServices(IServiceCollection services)
		{
			services.AddSingleton<IStorage, PostgreSQLStorage>();
			services.AddSingleton<IBootstrapper, PostgreSQLBootstrapper>();
			services.AddScoped<IStorageConnection, PostgreSQLStorageConnection>();

			services.AddSingleton<IAdditionalProcessor, PostgreSQLExpirationManager>();

			var PostgreSQLOptions = new PostgreSQLOptions();
			_configure(PostgreSQLOptions);

			services.AddSingleton(PostgreSQLOptions);
			services.AddSingleton<EFCoreOptions>(PostgreSQLOptions);

			services.AddDbContext<JobsDbContext>(options =>
			{
				options.UseNpgsql(PostgreSQLOptions.ConnectionString, sqlOpts =>
				{
					sqlOpts.MigrationsHistoryTable(
						PostgreSQLOptions.MigrationsHistoryTableName,
						PostgreSQLOptions.MigrationsHistoryTableSchema ?? PostgreSQLOptions.Schema);
				});
			});
		}
	}
}
