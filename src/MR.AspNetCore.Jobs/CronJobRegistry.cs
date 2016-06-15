using System;
using System.Collections.Generic;
using System.Reflection;
using NCrontab;

namespace MR.AspNetCore.Jobs
{
	public abstract class CronJobRegistry
	{
		private List<Entry> _entries;

		public CronJobRegistry()
		{
			_entries = new List<Entry>();
		}

		protected void RegisterJob<T>(string name, string cron) where T : IJob
		{
			RegisterJob(name, typeof(T), cron);
		}

		protected void RegisterJob(string name, Type jobType, string cron)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(cron));
			if (jobType == null) throw new ArgumentNullException(nameof(jobType));
			if (cron == null) throw new ArgumentNullException(nameof(cron));

			CrontabSchedule.TryParse(cron);

			if (!typeof(IJob).GetTypeInfo().IsAssignableFrom(jobType))
			{
				throw new ArgumentException(
					"Cron jobs should extend IJob.", nameof(jobType));
			}

			_entries.Add(new Entry(name, jobType, cron));
		}

		public Entry[] Build() => _entries.ToArray();

		public class Entry
		{
			public Entry(string name, Type jobType, string cron)
			{
				Name = name;
				JobType = jobType;
				Cron = cron;
			}

			public string Name { get; set; }

			public Type JobType { get; set; }

			public string Cron { get; set; }
		}
	}
}
