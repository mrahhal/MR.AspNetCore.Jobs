using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server;
using MR.AspNetCore.Jobs.Server.States;
using Xunit;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerStorageConnectionTest : DatabaseTestHost
	{
		[Fact]
		public async Task StoreJobAsync()
		{
			// Arrange
			var fixture = Create();
			var job = new Job("data", new DateTime(2000, 1, 1));

			// Act
			await fixture.StoreJobAsync(job);

			// Assert
			fixture.Context.Jobs.Any().Should().BeTrue();
		}

		[Fact]
		public async Task FetchNextJobAsync_ReadsPast()
		{
			// Arrange
			using (CreateScope())
			{
				var fixture = Create();
				var job1 = new Job("data");
				var job2 = new Job("data");
				await fixture.StoreJobAsync(job1);
				await fixture.StoreJobAsync(job2);

				fixture.Context.AddRange(new JobQueue { Job = job1 }, new JobQueue { Job = job2 });
				fixture.Context.SaveChanges();
			}

			// Act
			IFetchedJob fJob1 = null, fJob2 = null;
			using (var scope1 = CreateScope(Provider))
			using (var scope2 = CreateScope(Provider))
			{
				var fixture1 = Create(scope1.ServiceProvider);
				fJob1 = await fixture1.FetchNextJobAsync();

				var fixture2 = Create(scope2.ServiceProvider);
				fJob2 = await fixture2.FetchNextJobAsync();

				fJob1.RemoveFromQueue();
				fJob2.RemoveFromQueue();
			}

			// Assert
			fJob1.JobId.Should().NotBe(fJob2.JobId);

			using (CreateScope())
			{
				var fixture = Create();
				fixture.Context.JobQueue.Any().Should().BeFalse();
			}
		}

		[Fact]
		public async Task FetchNextJobAsync_NoJobs_ReturnsNull()
		{
			// Arrange
			var fixture = Create();

			// Act
			var result = await fixture.FetchNextJobAsync();

			// Assert
			result.Should().BeNull();
		}

		[Fact]
		public async Task FetchNextJobAsync_DueMinValue_Commit()
		{
			// Arrange
			var fixture = Create();
			var job = new Job("data", DateTime.MinValue);
			await fixture.StoreJobAsync(job);
			using (var t = fixture.CreateTransaction())
			{
				t.EnqueueJob(job);
				await t.CommitAsync();
			}

			// Act
			var result = await fixture.FetchNextJobAsync();
			result.RemoveFromQueue();
			result.Dispose();

			// Assert
			(await fixture.FetchNextJobAsync()).Should().BeNull();
		}

		[Fact]
		public async Task FetchNextJobAsync_Commit()
		{
			// Arrange
			var fixture = Create();
			var job = new Job("data");
			await fixture.StoreJobAsync(job);
			using (var t = fixture.CreateTransaction())
			{
				t.EnqueueJob(job);
				await t.CommitAsync();
			}

			// Act
			var result = await fixture.FetchNextJobAsync();
			result.RemoveFromQueue();
			result.Dispose();

			// Assert
			(await fixture.FetchNextJobAsync()).Should().BeNull();
		}

		[Fact]
		public async Task GetNextJobToBeEnqueuedAsync()
		{
			// Arrange
			var fixture = Create();
			var job = new Job("data", DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(10)));
			await fixture.StoreJobAsync(job);

			// Act
			var jobToBeEnqueued = await fixture.GetNextJobToBeEnqueuedAsync();

			// Assert
			jobToBeEnqueued.Id.Should().Be(job.Id);
		}

		[Fact]
		public async Task GetNextJobToBeEnqueuedAsync_NotDue()
		{
			// Arrange
			var fixture = Create();
			var job = new Job("data", DateTime.UtcNow.Add(TimeSpan.FromDays(1)));
			await fixture.StoreJobAsync(job);

			// Act
			var jobToBeEnqueued = await fixture.GetNextJobToBeEnqueuedAsync();

			// Assert
			jobToBeEnqueued.Should().BeNull();
		}

		[Fact]
		public async Task GetNextJobToBeEnqueuedAsync_InAStateOtherThanScheduled()
		{
			// Arrange
			var fixture = Create();
			var job = new Job("data", DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(10)))
			{
				StateName = EnqueuedState.StateName
			};
			await fixture.StoreJobAsync(job);

			// Act
			var jobToBeEnqueued = await fixture.GetNextJobToBeEnqueuedAsync();

			// Assert
			jobToBeEnqueued.Should().BeNull();
		}

		[Fact]
		public async Task StoreCronJobAsync()
		{
			// Arrange
			var fixture = Create();
			var cronJob = new CronJob
			{
				Cron = Cron.Daily(),
				Name = "1",
				TypeName = "foo",
				LastRun = new DateTime(2000, 1, 1)
			};

			// Act
			await fixture.StoreCronJobAsync(cronJob);

			// Assert
			fixture.Context.CronJobs.Any().Should().BeTrue();
		}

		[Fact]
		public async Task UpdateCronJobAsync()
		{
			// Arrange
			var fixture = Create();
			var cronJob = new CronJob
			{
				Cron = Cron.Daily(),
				Name = "1",
				TypeName = "foo",
				LastRun = new DateTime(2000, 1, 1)
			};
			await fixture.StoreCronJobAsync(cronJob);
			cronJob.LastRun = new DateTime(2001, 1, 1);
			cronJob.Cron = Cron.Minutely();

			// Act
			await fixture.UpdateCronJobAsync(cronJob);

			// Assert
			var job = fixture.Context.CronJobs.First();
			job.LastRun.Should().Be(cronJob.LastRun);
			job.Cron.Should().Be(cronJob.Cron);
		}

		[Fact]
		public async Task FetchNextJob_Requeue()
		{
			// Arrange
			var fixture = Create();
			var job = new Job("data", DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(10)));
			await fixture.StoreJobAsync(job);

			fixture.Context.Add(new JobQueue { Job = job });
			fixture.Context.SaveChanges();

			// Act
			var result = await fixture.FetchNextJobAsync();
			result.Requeue();
			result.Dispose();

			// Assert
			result = await fixture.FetchNextJobAsync();
			result.Should().NotBeNull();
			result.Dispose();
		}

		[Fact]
		public async Task GetCronJobsAsync()
		{
			// Arrange
			var fixture = Create();
			var cronJob1 = new CronJob
			{
				Cron = Cron.Daily(),
				Name = "1",
				TypeName = "foo",
				LastRun = new DateTime(2000, 1, 1)
			};
			var cronJob2 = new CronJob
			{
				Cron = Cron.Minutely(),
				Name = "2",
				TypeName = "bar",
				LastRun = new DateTime(2000, 1, 2)
			};
			await fixture.StoreCronJobAsync(cronJob1);
			await fixture.StoreCronJobAsync(cronJob2);

			// Act
			var result = await fixture.GetCronJobsAsync();

			// Assert
			result.Should().HaveCount(2);
		}

		protected override void PreBuildServices()
		{
			base.PreBuildServices();
			_services.AddScoped<SqlServerStorageConnection>();
		}

		private SqlServerStorageConnection Create(IServiceProvider provider = null)
		{
			provider = provider ?? Provider;
			return provider
				.GetService<SqlServerStorageConnection>();
		}
	}
}
