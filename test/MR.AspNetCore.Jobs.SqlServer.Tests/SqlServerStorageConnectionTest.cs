using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MR.AspNetCore.Jobs.Models;
using Xunit;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerStorageConnectionTest : DatabaseTestHost
	{
		[Fact]
		public async Task StoreJob_DelayedJob()
		{
			// Arrange
			var fixture = Create();
			var model = new Job("data", new DateTime(2000, 1, 1));

			// Act
			await fixture.StoreJobAsync(model);

			// Assert
			fixture._storage.UseConnection(connection =>
			{
				connection.Query<Job>("SELECT * FROM [Jobs].Jobs").Count().Should().NotBe(0);
			});
		}

		[Fact]
		public async Task StoreJob_CronJob()
		{
			// Arrange
			var fixture = Create();
			var model = new CronJob
			{
				Cron = Cron.Daily(),
				Name = "1",
				TypeName = "foo",
				LastRun = new DateTime(2000, 1, 1)
			};

			// Act
			await fixture.StoreCronJobAsync(model);

			// Assert
			fixture._storage.UseConnection(connection =>
			{
				connection.Query<CronJob>("SELECT * FROM [Jobs].CronJobs").Count().Should().NotBe(0);
			});
		}

		[Fact]
		public async Task UpdateCronJob()
		{
			// Arrange
			var fixture = Create();
			var model = new CronJob
			{
				Cron = Cron.Daily(),
				Name = "1",
				TypeName = "foo",
				LastRun = new DateTime(2000, 1, 1)
			};
			await fixture.StoreCronJobAsync(model);
			model.LastRun = new DateTime(2001, 1, 1);
			model.Cron = Cron.Minutely();

			// Act
			await fixture.UpdateCronJobAsync(model);

			// Assert
			fixture._storage.UseConnection(connection =>
			{
				var job = connection.Query<CronJob>("SELECT * FROM [Jobs].CronJobs").First();
				job.LastRun.Should().Be(model.LastRun);
				job.Cron.Should().Be(model.Cron);
			});
		}

		[Fact]
		public async Task FetchNextDelayedJobAsync_NoJobs_ReturnsNull()
		{
			// Arrange
			var fixture = Create();

			// Act
			var result = await fixture.FetchNextJobAsync();

			// Assert
			result.Should().BeNull();
		}

		[Fact]
		public async Task FetchNextDelayedJobAsync_Commit()
		{
			// Arrange
			var fixture = Create();
			var model = new Job("data");
			await fixture.StoreJobAsync(model);
			using (var t = fixture.CreateTransaction())
			{
				t.EnqueueJob(model.Id);
				await t.CommitAsync();
			}

			// Act
			var result = await fixture.FetchNextJobAsync();
			result.RemoveFromQueue();
			result.Dispose();

			// Assert
			(await fixture.FetchNextJobAsync()).Should().BeNull();
		}

		// Requeuing won't work directly here because we're using a transaction scope
		// to clean the database.
		//[Fact]
		//public void FetchNextJob_Requeue()
		//{
		//	// Arrange
		//	var fixture = Create();
		//	var model = new DelayedJob
		//	{
		//		Data = "data",
		//		Due = null
		//	};
		//	fixture.StoreJob(model);

		//	// Act
		//	var result = fixture.FetchNextJob();
		//	result.Requeue();
		//	result.Dispose();

		//	// Assert
		//	fixture.FetchNextJob().Should().NotBeNull();
		//}

		[Fact]
		public async Task FetchNextDelayedJobAsync_DueMinValue_Commit()
		{
			// Arrange
			var fixture = Create();
			var model = new Job("data", DateTime.MinValue);
			await fixture.StoreJobAsync(model);
			using (var t = fixture.CreateTransaction())
			{
				t.EnqueueJob(model.Id);
				await t.CommitAsync();
			}

			// Act
			var result = await fixture.FetchNextJobAsync();
			result.RemoveFromQueue();
			result.Dispose();

			// Assert
			(await fixture.FetchNextJobAsync()).Should().BeNull();
		}

		//[Fact]
		//public void FetchNextDelayedJob_Requeue()
		//{
		//	// Arrange
		//	var fixture = Create();
		//	var model = new DelayedJob
		//	{
		//		Data = "data",
		//		Due = DateTime.MinValue
		//	};
		//	fixture.StoreJob(model);

		//	// Act
		//	var result = fixture.FetchNextDelayedJob();
		//	result.Requeue();
		//	result.Dispose();

		//	// Assert
		//	fixture.FetchNextDelayedJob().Should().NotBeNull();
		//}

		[Fact]
		public async Task GetCronJobs()
		{
			// Arrange
			var fixture = Create();
			var model1 = new CronJob
			{
				Cron = Cron.Daily(),
				Name = "1",
				TypeName = "foo",
				LastRun = new DateTime(2000, 1, 1)
			};
			var model2 = new CronJob
			{
				Cron = Cron.Minutely(),
				Name = "2",
				TypeName = "bar",
				LastRun = new DateTime(2000, 1, 2)
			};
			await fixture.StoreCronJobAsync(model1);
			await fixture.StoreCronJobAsync(model2);

			// Act
			var result = await fixture.GetCronJobsAsync();

			// Assert
			result.Should().HaveCount(2);
		}

		private SqlServerStorageConnection Create()
		{
			var services = new ServiceCollection();
			services.AddOptions();
			services.AddLogging();
			services.AddTransient<SqlServerStorage>();
			services.Configure<SqlServerOptions>(
				options => options.ConnectionString = ConnectionUtil.GetConnectionString());
			return services.BuildServiceProvider()
				.GetService<SqlServerStorage>()
				.GetConnection() as SqlServerStorageConnection;
		}
	}
}
