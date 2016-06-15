using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerBootstrapper : IBootstrapper
	{
		private JobsOptions _options;
		private IStorage _storage;
		private IProcessingServer _server;
		private IApplicationLifetime _appLifetime;
		private ILoggerFactory _loggerFactory;

		public SqlServerBootstrapper(
			JobsOptions options,
			IStorage storage,
			IProcessingServer server,
			IApplicationLifetime appLifetime,
			ILoggerFactory loggerFactory)
		{
			_options = options;
			_storage = storage;
			_server = server;
			_appLifetime = appLifetime;
			_loggerFactory = loggerFactory;
		}

		public void Bootstrap()
		{
			_storage.Initialize();

			var registry = _options.CronJobRegistry;
			var types = default(CronJobRegistry.Entry[]);
			if (registry != null &&
				(types = registry.Build()).Length != 0)
			{
				using (var connection = _storage.GetConnection())
				{
					foreach (var entry in types)
					{
						var cronJob = connection.GetCronJobByName(entry.Name);
						var updating = cronJob != null;
						if (!updating)
						{
							cronJob = new CronJob
							{
								Name = entry.Name,
								TypeName = entry.JobType.AssemblyQualifiedName,
								Cron = entry.Cron,
								LastRun = DateTime.MinValue
							};
							connection.StoreJob(cronJob);
						}
						else
						{
							cronJob.Cron = entry.Cron;
							connection.UpdateCronJob(cronJob);
						}
					}
				}
			}

			_appLifetime.ApplicationStopping.Register(() => _server.Dispose());
			_server.Start();
		}
	}
}
