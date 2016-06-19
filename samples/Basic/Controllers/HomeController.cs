using System;
using System.Threading.Tasks;
using Basic.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MR.AspNetCore.Jobs;

namespace Basic.Controllers
{
	public class HomeController : Controller
	{
		private IJobsManager _jobs;
		private ILogger<HomeController> _logger;

		public HomeController(
			IJobsManager jobs,
			ILogger<HomeController> logger)
		{
			_jobs = jobs;
			_logger = logger;
		}

		public async Task<IActionResult> Index()
		{
			_logger.LogInformation("Enqueuing a job and having it execute after 5 secs.");
			await _jobs.EnqueueAsync<FooService>(
				fooService => fooService.LogSomething("Executing after a delay."),
				TimeSpan.FromSeconds(5));
			return View();
		}

		public async Task<IActionResult> About()
		{
			_logger.LogInformation("Enqueuing a job and having it execute immediately.");
			await _jobs.EnqueueAsync<FooService>(
				fooService => fooService.LogSomethingAsync("Executing immediately (in the background)."));
			return View();
		}

		public IActionResult Error()
		{
			return View();
		}
	}
}
