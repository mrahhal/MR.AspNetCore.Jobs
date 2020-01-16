using System.Threading.Tasks;
using Basic.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MR.AspNetCore.Jobs;

namespace Basic
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var host = CreateHostBuilder(args).Build();

			await host.StartJobsAsync();
			using (var scope = host.Services.CreateScope())
			{
				var context = scope.ServiceProvider.GetService<AppDbContext>();
				await context.Database.MigrateAsync();
			}

			host.Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
