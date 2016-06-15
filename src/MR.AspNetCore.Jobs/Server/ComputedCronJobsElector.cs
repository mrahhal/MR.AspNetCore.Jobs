using System;
using System.Collections.Generic;
using System.Linq;

namespace MR.AspNetCore.Jobs.Server
{
	public class ComputedCronJobsElector
	{
		public ComputedCronJob Elect(IEnumerable<ComputedCronJob> jobs, DateTime? baseTime = null)
		{
			var now = baseTime ?? DateTime.UtcNow;
			foreach (var c in jobs)
			{
				// We want old jobs that have passed their scheduled execution time
				// to be elected first.
				var next = c.Schedule.GetNextOccurrence(now);
				var previousNext = c.Schedule.GetNextOccurrence(c.Job.LastRun);
				c.Next = next > previousNext ? now : next;
			}

			// Election between old jobs that didn't execute isn't determined.
			return jobs.OrderBy(c => c.Next).First();
		}
	}
}
