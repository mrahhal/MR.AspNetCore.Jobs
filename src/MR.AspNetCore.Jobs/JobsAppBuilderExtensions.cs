using Microsoft.Extensions.DependencyInjection;
using MR.AspNetCore.Jobs;

namespace Microsoft.AspNetCore.Builder
{
	public static class JobsAppBuilderExtensions
	{
		public static void UseJobs(this IApplicationBuilder app)
		{
			var provider = app.ApplicationServices;
			var bootstrapper = provider.GetRequiredService<IBootstrapper>();
			bootstrapper.BootstrapAsync();
		}
	}
}
