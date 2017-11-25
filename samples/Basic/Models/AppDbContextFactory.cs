using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Basic.Models
{
	public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
	{
		public AppDbContext CreateDbContext(string[] args)
		{
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.Build();

			var services = new ServiceCollection();

			services.AddDbContext<AppDbContext>(opts =>
			{
				opts.UseSqlServer(configuration["ConnectionStrings:Default"]);
			});

			return services.BuildServiceProvider().GetRequiredService<AppDbContext>();
		}
	}
}
