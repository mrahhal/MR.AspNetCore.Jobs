using System;

namespace MR.AspNetCore.Jobs.Models
{
	public class DelayedJob
	{
		public DelayedJob()
		{
			Id = Guid.NewGuid().ToString();
		}

		public string Id { get; set; }
		public string Data { get; set; }
		public DateTime? Due { get; set; }
	}
}
