using System;
using FluentAssertions;
using MR.AspNetCore.Jobs.Models;
using Xunit;

namespace MR.AspNetCore.Jobs.Server
{
	public class ComputedCronJobsElectorTest
	{
		[Fact]
		public void Elect_OldJobsThatDidntGetToExecuteComesOutFirst()
		{
			// Arrange
			var calculator = Create();
			var jobs = new ComputedCronJob[]
			{
				new ComputedCronJob (new CronJob
				{
					Cron = Cron.Hourly(12),
					Name = "1",
					LastRun = new DateTime(2000, 1, 1, 0, 11, 0)
				}),
				new ComputedCronJob (new CronJob
				{
					Cron = Cron.Hourly(13),
					Name = "2",
					LastRun = new DateTime(2001, 1, 1, 0, 13, 0)
				})
			};
			var baseTime = new DateTime(2001, 1, 1, 0, 14, 0);

			// Act
			var result = calculator.Elect(jobs, baseTime);

			// Assert
			result.Should().Be(jobs[0]);
			result.Next.Should().Equals(baseTime); // Next should be set to baseTime
		}

		[Fact]
		public void Elect()
		{
			// Arrange
			var calculator = Create();
			var jobs = new ComputedCronJob[]
			{
				new ComputedCronJob (new CronJob
				{
					Cron = Cron.Hourly(12),
					Name = "1",
					LastRun = new DateTime(2001, 1, 1, 0, 12, 0)
				}),
				new ComputedCronJob (new CronJob
				{
					Cron = Cron.Hourly(16),
					Name = "2",
					LastRun = new DateTime(2001, 1, 1, 0, 16, 0)
				})
			};
			var baseTime = new DateTime(2001, 1, 1, 1, 10, 0);

			// Act
			var result = calculator.Elect(jobs, baseTime);

			// Assert
			result.Should().Be(jobs[0]);
			(result.Next - baseTime).TotalMinutes.Should().Be(2);
		}

		private ComputedCronJobsElector Create() => new ComputedCronJobsElector();
	}
}
