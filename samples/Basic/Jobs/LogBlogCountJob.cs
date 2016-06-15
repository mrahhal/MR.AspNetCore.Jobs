using System.Threading.Tasks;
using Basic.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MR.AspNetCore.Jobs;

namespace Basic.Jobs
{
	public class LogBlogCountJob : IJob
	{
		private AppDbContext _context;
		private ILogger<LogBlogCountJob> _logger;

		public LogBlogCountJob(
			AppDbContext context,
			ILogger<LogBlogCountJob> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task ExecuteAsync()
		{
			_logger.LogInformation($"Executing {nameof(LogBlogCountJob)}.");
			var count = await _context.Blogs.CountAsync();
			_logger.LogInformation($"There are {count} blog(s).");
		}
	}
}
