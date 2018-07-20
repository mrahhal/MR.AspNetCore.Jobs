using MR.AspNetCore.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class JobsOptionsExtensions
	{
		/// <summary>
		/// Registers the cron jobs registries within the assembly
		/// </summary>
		/// <param name="assembly">Assembly containing classes extending from <see cref="CronJobRegistry"/>.</param>
		public static void UseCronJobRegistries(this JobsOptions jobsOptions, Assembly assembly)
		{
			// Get all classes extending cron job registry
			var types = assembly.GetTypes().Where(type => typeof(CronJobRegistry).IsAssignableFrom(type));

			foreach (var type in types)
			{
				var registry = Activator.CreateInstance(type) as CronJobRegistry;
				jobsOptions.UseCronJobRegistry(registry);
			}
		}

		/// <summary>
		/// Registers the cron jobs registries within the assembly
		/// </summary>
		/// <param name="assemblies">Assembly containing classes extending from <see cref="CronJobRegistry"/>.</param>
		public static void UseCronJobRegistries(this JobsOptions jobsOptions, IEnumerable<Assembly> assemblies)
		{
			foreach (var assembly in assemblies)
			{
				jobsOptions.UseCronJobRegistries(assembly);
			}
		}
	}
}
