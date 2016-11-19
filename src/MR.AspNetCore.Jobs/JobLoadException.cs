using System;

namespace MR.AspNetCore.Jobs
{
	public class JobLoadException : Exception
	{
		public JobLoadException()
		{
		}

		public JobLoadException(string message) : base(message)
		{
		}

		public JobLoadException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
