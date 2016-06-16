using System;

namespace MR.AspNetCore.Jobs
{
	public class JobsOptions
	{
		internal IJobsOptionsExtension Extension { get; private set; }

		public CronJobRegistry CronJobRegistry { get; private set; }

		public void UseCronJobRegistry<T>()
			where T : CronJobRegistry
		{
			var registry = Activator.CreateInstance<T>();
			UseCronJobRegistry(registry);
		}

		public void UseCronJobRegistry(CronJobRegistry registry)
		{
			if (registry == null) throw new ArgumentNullException(nameof(registry));

			CronJobRegistry = registry;
		}

		public void RegisterExtension(IJobsOptionsExtension extension)
		{
			if (extension == null) throw new ArgumentNullException(nameof(extension));

			Extension = extension;
		}
	}
}
