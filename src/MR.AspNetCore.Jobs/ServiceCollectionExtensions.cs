using System;
using Microsoft.Extensions.DependencyInjection;
using MR.AspNetCore.Jobs.Server;

namespace MR.AspNetCore.Jobs
{
	public static class ServiceCollectionExtensions
	{
		public static void AddJobs(
			this IServiceCollection services,
			Action<JobsOptions> configure)
		{
			services.AddSingleton<IJobsManager, JobsManager>();
			services.AddSingleton<IJobFactory, JobFactory>();
			services.AddSingleton<IProcessingServer, ProcessingServer>();

			// Jobs
			services.AddTransient<FireAndForgetJobProcessor>();
			services.AddTransient<DelayedJobProcessor>();
			services.AddTransient<CronJobProcessor>();

			var options = new JobsOptions();
			configure(options);
			options.Extension?.AddServices(services);
			services.AddSingleton(options);
		}
	}
}
