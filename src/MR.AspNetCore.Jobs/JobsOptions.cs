using System;

namespace MR.AspNetCore.Jobs
{
	public class JobsOptions
	{
		internal IJobsOptionsExtension Extension { get; private set; }

		/// <summary>
		/// Gets the <see cref="CronJobRegistry"/> that will be used.
		/// </summary>
		public CronJobRegistry CronJobRegistry { get; private set; }

		/// <summary>
		/// Gets or sets the polling delay in seconds used by the processors when polling the server.
		/// </summary>
		public int PollingDelay { get; set; } = 15;

		/// <summary>
		/// Sets the <see cref="CronJobRegistry"/> to use.
		/// </summary>
		/// <typeparam name="T">The type of the <see cref="CronJobRegistry"/>.</typeparam>
		public void UseCronJobRegistry<T>()
			where T : CronJobRegistry
		{
			var registry = Activator.CreateInstance<T>();
			UseCronJobRegistry(registry);
		}

		/// <summary>
		/// Sets the <see cref="CronJobRegistry"/> to use.
		/// </summary>
		/// <param name="registry">The <see cref="CronJobRegistry"/>.</param>
		public void UseCronJobRegistry(CronJobRegistry registry)
		{
			if (registry == null) throw new ArgumentNullException(nameof(registry));

			CronJobRegistry = registry;
		}

		/// <summary>
		/// Registers an extension that will be executed when building services.
		/// </summary>
		/// <param name="extension"></param>
		public void RegisterExtension(IJobsOptionsExtension extension)
		{
			if (extension == null) throw new ArgumentNullException(nameof(extension));

			Extension = extension;
		}
	}
}
