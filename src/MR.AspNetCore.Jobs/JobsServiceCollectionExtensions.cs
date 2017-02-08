using System;
using MR.AspNetCore.Jobs;
using MR.AspNetCore.Jobs.Server;
using MR.AspNetCore.Jobs.Server.States;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class JobsServiceCollectionExtensions
	{
		public static void AddJobs(
			this IServiceCollection services,
			Action<JobsOptions> configure)
		{
			services.AddScoped<IJobsManager, JobsManager>();
			services.AddSingleton<IJobFactory, JobFactory>();
			services.AddSingleton<IProcessingServer, ProcessingServer>();
			services.AddSingleton<IStateChanger, StateChanger>();

			// Processors
			services.AddTransient<DelayedJobProcessor>();
			services.AddTransient<CronJobProcessor>();
			services.AddTransient<JobQueuer>();

			var options = new JobsOptions();
			configure(options);
			options.Extension?.AddServices(services);
			services.AddSingleton(options);
		}
	}
}
