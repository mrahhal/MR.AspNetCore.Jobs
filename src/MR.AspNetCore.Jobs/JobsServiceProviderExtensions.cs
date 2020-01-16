using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MR.AspNetCore.Jobs
{
	public static class JobsWebHostExtensions
	{
		public static Task StartJobsAsync(this IHost host)
		{
			var bootstrapper = host.Services.GetRequiredService<IBootstrapper>();
			return bootstrapper.BootstrapAsync();
		}
	}
}
