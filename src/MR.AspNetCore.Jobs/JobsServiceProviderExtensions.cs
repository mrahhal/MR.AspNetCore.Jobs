using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MR.AspNetCore.Jobs
{
	public static class JobsWebHostExtensions
	{
		public static Task StartJobsAsync(this IWebHost host)
		{
			var bootstrapper = host.Services.GetRequiredService<IBootstrapper>();
			return bootstrapper.BootstrapAsync();
		}
	}
}
