using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
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

		public SqlServerBootstrapper(
			JobsOptions options,
			IStorage storage,
			IProcessingServer server,
			IApplicationLifetime appLifetime)
		{
			_options = options;
			_storage = storage;
			_server = server;
			_appLifetime = appLifetime;
		}

		public void Bootstrap()
		{
			_storage.Initialize();

			using (var connection = _storage.GetConnection())
			{
				var registry = _options.CronJobRegistry;
				var entries = default(CronJobRegistry.Entry[]);
				var existingJobs = connection.GetCronJobs();
				if (registry != null &&
					(entries = registry.Build()).Length != 0)
				{
					// Add or update jobs
					foreach (var entry in entries)
					{
						var cronJob = existingJobs.FirstOrDefault(j => j.Name == entry.Name);
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
							cronJob.TypeName = entry.JobType.AssemblyQualifiedName;
							cronJob.Cron = entry.Cron;
							connection.UpdateCronJob(cronJob);
						}
					}
				}

				// Delete old jobs
				foreach (var oldJob in existingJobs.Where(j => !entries.Any(e => e.Name == j.Name)))
				{
					connection.RemoveCronJob(oldJob.Name);
				}
			}

			_appLifetime.ApplicationStopping.Register(() => _server.Dispose());
			_server.Start();
		}
	}
}
