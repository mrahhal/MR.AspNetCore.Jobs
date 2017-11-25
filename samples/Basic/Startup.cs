using Basic.Jobs;
using Basic.Models;
using Basic.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Basic
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<AppDbContext>(options => options.UseSqlServer(Configuration["ConnectionStrings:Default"]));
			services.AddMvc();

			services.AddJobs(options =>
			{
				// Use the sql server adapter
				options.UseSqlServer(opts =>
				{
					opts.ConnectionString = Configuration["ConnectionStrings:Default"];

					// This is the default schema used.
					//opts.Schema = "Jobs";
				});

				// Use the cron jobs registry
				options.UseCronJobRegistry<BasicCronJobRegistry>();

				// Configure the polling delay used when polling the storage (in seconds)
				options.PollingDelay = 10;
			});

			// Add jobs to DI
			services.AddTransient<LogBlogCountJob>();
			services.AddTransient<RetryableJob>();

			// Services
			services.AddScoped<FooService>();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
