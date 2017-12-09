using System;

namespace MR.AspNetCore.Jobs.Models
{
	public class JobQueue
	{
		public int Id { get; set; }

		public int JobId { get; set; }
		public Job Job { get; set; }

		public DateTime? FetchedAt { get; set; }
	}
}
