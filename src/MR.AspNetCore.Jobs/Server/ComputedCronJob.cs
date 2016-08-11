using System;
using MR.AspNetCore.Jobs.Models;
using NCrontab;

namespace MR.AspNetCore.Jobs.Server
{
	public class ComputedCronJob
	{
		private CronJobRegistry.Entry _entry;

		public ComputedCronJob()
		{
		}

		public ComputedCronJob(CronJob job, CronJobRegistry.Entry entry)
		{
			_entry = entry;
			Job = job;
			Schedule = CrontabSchedule.Parse(job.Cron);
			if (job.TypeName != null)
				JobType = Type.GetType(job.TypeName);
		}

		public CronJob Job { get; set; }

		public CrontabSchedule Schedule { get; set; }

		public Type JobType { get; set; }

		public DateTime Next { get; set; }

		public int Retries { get; set; }

		public DateTime FirstTry { get; set; }

		public RetryBehavior RetryBehavior => _entry.RetryBehavior;

		public void Update(DateTime baseTime)
		{
			Job.LastRun = baseTime;
		}

		public void UpdateNext(DateTime now)
		{
			var next = Schedule.GetNextOccurrence(now);
			var previousNext = Schedule.GetNextOccurrence(Job.LastRun);
			Next = next > previousNext ? now : next;
		}
	}
}
