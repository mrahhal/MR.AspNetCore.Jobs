using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server;

namespace MR.AspNetCore.Jobs
{
	public abstract class BootstrapperBase : IBootstrapper
	{
		private IHostApplicationLifetime _appLifetime;
		private CancellationTokenSource _cts;
		private CancellationTokenRegistration _ctsRegistration;
		private Task _bootstrappingTask;

		public BootstrapperBase(
			JobsOptions options,
			IStorage storage,
			IProcessingServer server,
			IHostApplicationLifetime appLifetime,
			IServiceProvider provider)
		{
			Options = options;
			Storage = storage;
			Server = server;
			Provider = provider;
			_appLifetime = appLifetime;

			_cts = new CancellationTokenSource();
			_ctsRegistration = appLifetime.ApplicationStopping.Register(() =>
			{
				_cts.Cancel();
				try
				{
					_bootstrappingTask?.Wait();
				}
				catch (OperationCanceledException)
				{
				}
			});
		}

		protected JobsOptions Options { get; }

		protected IStorage Storage { get; }

		protected IProcessingServer Server { get; }

		protected IServiceProvider Provider { get; }

		public Task BootstrapAsync()
		{
			return (_bootstrappingTask = BootstrapTaskAsync());
		}

		private async Task BootstrapTaskAsync()
		{
			await Storage.InitializeAsync(_cts.Token);
			if (_cts.IsCancellationRequested) return;

			await SyncCronJobsAsync();
			if (_cts.IsCancellationRequested) return;

			await BootstrapCoreAsync();
			if (_cts.IsCancellationRequested) return;

			Server.Start();

			_ctsRegistration.Dispose();
			_cts.Dispose();
		}

		public async Task SyncCronJobsAsync()
		{
			var entries = Options.CronJobRegistry?.Build() ?? new CronJobRegistry.Entry[0];
			using (var scope = Provider.CreateScope())
			{
				var provider = scope.ServiceProvider;
				var connection = provider.GetService<IStorageConnection>();

				var currentJobs = await connection.GetCronJobsAsync();
				await SyncCronJobsCoreAsync(connection, entries, currentJobs);
			}
		}

		public virtual async Task SyncCronJobsCoreAsync(
			IStorageConnection connection,
			CronJobRegistry.Entry[] entries,
			CronJob[] currentJobs)
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
						await connection.StoreCronJobAsync(cronJob);
					}
					else
					{
						cronJob.TypeName = entry.JobType.AssemblyQualifiedName;
						cronJob.Cron = entry.Cron;
						await connection.UpdateCronJobAsync(cronJob);
					}
				}
			}

			// Delete old jobs
			foreach (var oldJob in currentJobs.Where(j => !entries.Any(e => e.Name == j.Name)))
			{
				await connection.RemoveCronJobAsync(oldJob.Name);
			}
		}

		public virtual Task BootstrapCoreAsync()
		{
			_appLifetime.ApplicationStopping.Register(() => Server.Dispose());
			return Task.CompletedTask;
		}
	}
}
