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
			var model = new DelayedJob
			{
				Data = "data",
				Due = new DateTime(2000, 1, 1)
			};

			// Act
			await fixture.StoreJobAsync(model);

			// Assert
			fixture._storage.UseConnection(connection =>
			{
				connection.Query<DelayedJob>("SELECT * FROM Jobs.DelayedJobs").Count().Should().NotBe(0);
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
			await fixture.StoreJobAsync(model);

			// Assert
			fixture._storage.UseConnection(connection =>
			{
				connection.Query<CronJob>("SELECT * FROM Jobs.CronJobs").Count().Should().NotBe(0);
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
			await fixture.StoreJobAsync(model);
			model.LastRun = new DateTime(2001, 1, 1);
			model.Cron = Cron.Minutely();

			// Act
			await fixture.UpdateCronJobAsync(model);

			// Assert
			fixture._storage.UseConnection(connection =>
			{
				var job = connection.Query<CronJob>("SELECT * FROM Jobs.CronJobs").First();
				job.LastRun.Should().Be(model.LastRun);
				job.Cron.Should().Be(model.Cron);
			});
		}

		[Fact]
		public async Task FetchNextJob_NoJobs_ReturnsNull()
		{
			// Arrange
			var fixture = Create();

			// Act
			var result = await fixture.FetchNextJobAsync();

			// Assert
			result.Should().BeNull();
		}

		[Fact]
		public async Task FetchNextJob_Commit()
		{
			// Arrange
			var fixture = Create();
			var model = new DelayedJob
			{
				Data = "data",
				Due = null
			};
			await fixture.StoreJobAsync(model);

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
		public async Task FetchNextDelayedJob_NoJobs_ReturnsNull()
		{
			// Arrange
			var fixture = Create();

			// Act
			var result = await fixture.FetchNextDelayedJob();

			// Assert
			result.Should().BeNull();
		}

		[Fact]
		public async Task FetchNextDelayedJob_Commit()
		{
			// Arrange
			var fixture = Create();
			var model = new DelayedJob
			{
				Data = "data",
				Due = DateTime.MinValue
			};
			await fixture.StoreJobAsync(model);

			var jobs = fixture._storage.UseConnection(c =>
			{
				return c.Query<DelayedJob>("SELECT * FROM Jobs.DelayedJobs").ToArray();
			});

			// Act
			var result = await fixture.FetchNextDelayedJob();
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
			await fixture.StoreJobAsync(model1);
			await fixture.StoreJobAsync(model2);

			// Act
			var result = await fixture.GetCronJobsAsync();

			// Assert
			result.Should().HaveCount(2);
		}

		[Fact]
		public async Task GetCronJobByName()
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
			await fixture.StoreJobAsync(model1);
			await fixture.StoreJobAsync(model2);

			// Act
			var result = await fixture.GetCronJobByNameAsync("2");

			// Assert
			result.Should().NotBeNull();
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
