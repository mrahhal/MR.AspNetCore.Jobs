using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server.States;
using Xunit;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerStorageConnectionTest : DatabaseTestHost
	{
		private IServiceProvider _provider;

		public SqlServerStorageConnectionTest()
		{
			var services = new ServiceCollection();
			services.AddOptions();
			services.AddLogging();
			services.AddTransient<SqlServerStorage>();
			services.Configure<SqlServerOptions>(
				options => options.ConnectionString = ConnectionUtil.GetConnectionString());
			_provider = services.BuildServiceProvider();
		}

		[Fact]
		public async Task StoreJobAsync()
		{
			// Arrange
			var fixture = Create();
			var job = new Job("data", new DateTime(2000, 1, 1));

			// Act
			await fixture.StoreJobAsync(job);

			// Assert
			fixture._storage.UseConnection(connection =>
			{
				connection.Query<Job>("SELECT * FROM [Jobs].Jobs").Count().Should().NotBe(0);
			});
		}

		[Fact]
		public async Task FetchNextJobAsync_ReadsPast()
		{
			// Arrange
			var fixture = Create();
			var job1 = new Job("data");
			var job2 = new Job("data");
			await fixture.StoreJobAsync(job1);
			await fixture.StoreJobAsync(job2);

			fixture._storage.UseConnection(connection =>
			{
				connection.Execute("INSERT INTO [Jobs].JobQueue (JobId) VALUES (@Id)", new[] { job1, job2 });
			});

			// Act
			var fJob1 = await fixture.FetchNextJobAsync();
			var fJob2 = await fixture.FetchNextJobAsync();
			fJob1.RemoveFromQueue();
			fJob2.RemoveFromQueue();

			// Assert
			fJob1.JobId.Should().NotBe(fJob2.JobId);
			fixture._storage.UseConnection(connection =>
			{
				connection.Query<Job>("SELECT * FROM [Jobs].JobQueue").Count().Should().Be(0);
			});
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
				t.EnqueueJob(job.Id);
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
				t.EnqueueJob(job.Id);
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
			var job = new Job("data", DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(10)));
			job.StateName = EnqueuedState.StateName;
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
			fixture._storage.UseConnection(connection =>
			{
				connection.Query<CronJob>("SELECT * FROM [Jobs].CronJobs").Count().Should().NotBe(0);
			});
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
			fixture._storage.UseConnection(connection =>
			{
				var job = connection.Query<CronJob>("SELECT * FROM [Jobs].CronJobs").First();
				job.LastRun.Should().Be(cronJob.LastRun);
				job.Cron.Should().Be(cronJob.Cron);
			});
		}

		// Requeuing won't work directly here because we're using a transaction scope
		// to clean the database.
		/*[Fact]
		public async Task FetchNextJob_Requeue()
		{
			// Arrange
			var fixture = Create();
			var job = new Job("data", DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(10)));
			await fixture.StoreJobAsync(job);

			fixture._storage.UseConnection(connection =>
			{
				connection.Execute("INSERT INTO [Jobs].JobQueue (JobId) VALUES (@Id)", new[] { job });
			});

			// Act
			var result = await fixture.FetchNextJobAsync();
			result.Requeue();
			result.Dispose();

			// Assert
			(await fixture.FetchNextJobAsync()).Should().NotBeNull();
		}*/

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

		private SqlServerStorageConnection Create()
		{
			return _provider
				.GetService<SqlServerStorage>()
				.GetConnection() as SqlServerStorageConnection;
		}
	}
}
