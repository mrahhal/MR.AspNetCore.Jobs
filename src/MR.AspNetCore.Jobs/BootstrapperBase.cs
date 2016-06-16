using System;
using System.Linq;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server;

namespace MR.AspNetCore.Jobs
{
	public abstract class BootstrapperBase : IBootstrapper
	{
		public BootstrapperBase(
			JobsOptions options,
			IStorage storage,
			IProcessingServer server)
		{
			Options = options;
			Storage = storage;
			Server = server;
		}

		protected JobsOptions Options { get; }

		protected IStorage Storage { get; }

		protected IProcessingServer Server { get; }

		public void Bootstrap()
		{
			Storage.Initialize();
			WorkOutCronJobs();
			BootstrapCore();
			Server.Start();
		}

		public void WorkOutCronJobs()
		{
			var entries = Options.CronJobRegistry?.Build() ?? Enumerable.Empty<CronJobRegistry.Entry>().ToArray();
			using (var connection = Storage.GetConnection())
			{
				var currentJobs = connection.GetCronJobs();
				WorkOutCronJobsCore(connection, entries, currentJobs);
			}
		}

		public virtual void WorkOutCronJobsCore(IStorageConnection connection, CronJobRegistry.Entry[] entries, CronJob[] currentJobs)
		{
			if (entries.Length != 0)
			{
				// Add or update jobs
				foreach (var entry in entries)
				{
					var cronJob = currentJobs.FirstOrDefault(j => j.Name == entry.Name);
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
			foreach (var oldJob in currentJobs.Where(j => !entries.Any(e => e.Name == j.Name)))
			{
				connection.RemoveCronJob(oldJob.Name);
			}
		}

		public virtual void BootstrapCore()
		{
		}
	}
}
