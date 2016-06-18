using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MR.AspNetCore.Jobs
{
	public static class AppBuilderExtensions
	{
		public static void UseJobs(this IApplicationBuilder app)
		{
			var provider = app.ApplicationServices;
			var bootstrapper = provider.GetRequiredService<IBootstrapper>();
			bootstrapper.BootstrapAsync();
		}
	}
}
