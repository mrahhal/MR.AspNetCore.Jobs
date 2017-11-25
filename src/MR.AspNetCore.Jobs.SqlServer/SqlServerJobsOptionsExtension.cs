using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerJobsOptionsExtension : IJobsOptionsExtension
	{
		private Action<SqlServerOptions> _configure;

		public SqlServerJobsOptionsExtension(Action<SqlServerOptions> configure)
		{
			_configure = configure;
		}

		public void AddServices(IServiceCollection services)
		{
			services.AddSingleton<IStorage, SqlServerStorage>();
			services.AddSingleton<IBootstrapper, SqlServerBootstrapper>();
			services.AddScoped<IStorageConnection, SqlServerStorageConnection>();

			services.AddSingleton<IAdditionalProcessor, SqlServerExpirationManager>();

			var sqlServerOptions = new SqlServerOptions();
			_configure(sqlServerOptions);

			services.AddSingleton(sqlServerOptions);
			services.AddSingleton<EFCoreOptions>(sqlServerOptions);

			services.AddDbContext<JobsDbContext>(options =>
			{
				options.UseSqlServer(sqlServerOptions.ConnectionString, sqlOpts =>
				{
					sqlOpts.MigrationsHistoryTable(
						sqlServerOptions.MigrationsHistoryTableName,
						sqlServerOptions.MigrationsHistoryTableSchema ?? sqlServerOptions.Schema);
				});
			});
		}
	}
}
