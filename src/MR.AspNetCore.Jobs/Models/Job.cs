using System;
using MR.AspNetCore.Jobs.Server.States;

namespace MR.AspNetCore.Jobs.Models
{
	/// <summary>
	/// Represents a job to be executed at a later time.
	/// </summary>
	public class Job
	{
		public Job()
		{
			Added = DateTime.UtcNow;
			StateName = ScheduledState.StateName;
		}

		public Job(string data)
			: this()
		{
			Data = data;
		}

		public Job(string data, DateTime due)
			: this(data)
		{
			Due = due;
		}

		public int Id { get; set; }

		public string Data { get; set; }

		public DateTime Added { get; set; }

		public DateTime? Updated { get; set; }

		public DateTime? Due { get; set; }

		public DateTime? ExpiresAt { get; set; }

		public int Retries { get; set; }

		public string StateName { get; set; }
	}
}
