using System;
using MR.AspNetCore.Jobs.Models;
using NCrontab;

namespace MR.AspNetCore.Jobs.Server
{
	public class ComputedCronJob
	{
		public ComputedCronJob()
		{
		}

		public ComputedCronJob(CronJob job)
		{
			Job = job;
			Schedule = CrontabSchedule.Parse(job.Cron);
			if (job.TypeName != null)
				JobType = Type.GetType(job.TypeName);
		}

		public CronJob Job { get; set; }

		public CrontabSchedule Schedule { get; set; }

		public Type JobType { get; set; }

		public DateTime Next { get; set; }

		public void Update(DateTime baseTime)
		{
			Job.LastRun = baseTime;
		}
	}
}
