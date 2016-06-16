using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerBootstrapper : BootstrapperBase
	{
		private IApplicationLifetime _appLifetime;

		public SqlServerBootstrapper(
			JobsOptions options,
			IStorage storage,
			IProcessingServer server,
			IApplicationLifetime appLifetime)
			: base(options, storage, server)
		{
			_appLifetime = appLifetime;
		}

		public override void BootstrapCore()
		{
			using (var connection = Storage.GetConnection())
			{
				var registry = Options.CronJobRegistry;
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

			_appLifetime.ApplicationStopping.Register(() => Server.Dispose());
		}
	}
}
