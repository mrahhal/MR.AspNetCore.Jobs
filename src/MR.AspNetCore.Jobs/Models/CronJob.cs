using System;

namespace MR.AspNetCore.Jobs.Models
{
	/// <summary>
	/// Represents a cron job to be executed at specified intervals of time.
	/// </summary>
	public class CronJob
	{
		public CronJob()
		{
			Id = Guid.NewGuid().ToString();
		}

		public string Id { get; set; }
		public string Name { get; set; }
		public string TypeName { get; set; }
		public string Cron { get; set; }
		public DateTime LastRun { get; set; }
	}
}
