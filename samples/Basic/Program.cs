using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using MR.AspNetCore.Jobs;

namespace Basic
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var host = BuildWebHost(args);

			await host.StartJobsAsync();

			host.Run();
		}

		public static IWebHost BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
				.Build();
	}
}
