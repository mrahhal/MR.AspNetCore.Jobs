using System;

namespace Microsoft.AspNetCore.Builder
{
	public static class JobsAppBuilderExtensions
	{
		private const string Message =
			"Use host.StartJobsAsync() when building your webhost instead. Check the samples for more.";

		[Obsolete(Message)]
		public static void UseJobs(this IApplicationBuilder app)
		{
			throw new NotImplementedException(Message);
		}
	}
}
