using System;

namespace MR.AspNetCore.Jobs.Models
{
	/// <summary>
	/// Represents a delayed job to be executed at a later time.
	/// </summary>
	public class DelayedJob
	{
		public DelayedJob()
		{
			Id = Guid.NewGuid().ToString();
			Added = DateTime.UtcNow;
		}

		public DelayedJob(string data)
			: this()
		{
			Data = data;
		}

		public string Id { get; set; }
		public string Data { get; set; }
		public DateTime Added { get; set; }
	}
}
