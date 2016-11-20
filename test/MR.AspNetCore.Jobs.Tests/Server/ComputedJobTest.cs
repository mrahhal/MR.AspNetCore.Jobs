using System;
using FluentAssertions;
using MR.AspNetCore.Jobs.Models;
using Xunit;

namespace MR.AspNetCore.Jobs.Server
{
	public class ComputedJobTest
	{
		[Fact]
		public void UpdateNext_LastRunNever_SchedulesNow()
		{
			// Arrange
			var now = new DateTime(2000, 1, 1, 8, 0, 0);
			var cronJob = new CronJob(Cron.Daily());
			var computed = new ComputedCronJob(cronJob);

			// Act
			computed.UpdateNext(now);

			// Assert
			computed.Next.Should().Be(now);
		}

		[Fact]
		public void UpdateNext_LastRun_BeforePrev_SchedulesNow()
		{
			// Arrange
			var now = new DateTime(2000, 1, 1, 8, 0, 0);
			var cronJob = new CronJob(Cron.Daily(), now.Subtract(TimeSpan.FromDays(2)));
			var computed = new ComputedCronJob(cronJob);

			// Act
			computed.UpdateNext(now);

			// Assert
			computed.Next.Should().Be(now);
		}

		[Fact]
		public void UpdateNext_LastRun_AfterPrev_SchedulesNormal()
		{
			// Arrange
			var now = new DateTime(2000, 1, 1, 8, 0, 0);
			var cronJob = new CronJob(Cron.Daily(), now.Subtract(TimeSpan.FromSeconds(5)));
			var computed = new ComputedCronJob(cronJob);

			// Act
			computed.UpdateNext(now);

			// Assert
			computed.Next.Should().BeAfter(now);
		}
	}
}
