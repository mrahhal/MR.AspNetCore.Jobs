using MR.AspNetCore.Jobs;

namespace Basic.Jobs
{
	public class BasicCronJobRegistry : CronJobRegistry
	{
		public BasicCronJobRegistry()
		{
			RegisterJob<LogBlogCountJob>(nameof(LogBlogCountJob), Cron.Minutely());
		}
	}
}
