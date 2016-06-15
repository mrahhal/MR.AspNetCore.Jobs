using System;
using System.Linq;
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
		public void StoreJob_DelayedJob()
		{
			// Arrange
			var fixture = Create();
			var model = new DelayedJob
			{
				Data = "data",
				Due = new DateTime(2000, 1, 1)
			};

			// Act
			fixture.StoreJob(model);

			// Assert
			fixture._storage.UseConnection(connection =>
			{
				connection.Query<DelayedJob>("SELECT * FROM Jobs.DelayedJobs").Count().Should().NotBe(0);
			});
		}

		[Fact]
		public void StoreJob_CronJob()
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
			fixture.StoreJob(model);

			// Assert
			fixture._storage.UseConnection(connection =>
			{
				connection.Query<CronJob>("SELECT * FROM Jobs.CronJobs").Count().Should().NotBe(0);
			});
		}

		[Fact]
		public void UpdateCronJob()
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
			fixture.StoreJob(model);
			model.LastRun = new DateTime(2001, 1, 1);
			model.Cron = Cron.Minutely();

			// Act
			fixture.UpdateCronJob(model);

			// Assert
			fixture._storage.UseConnection(connection =>
			{
				var job = connection.Query<CronJob>("SELECT * FROM Jobs.CronJobs").First();
				job.LastRun.Should().Be(model.LastRun);
				job.Cron.Should().Be(model.Cron);
			});
		}

		[Fact]
		public void FetchNextJob_NoJobs_ReturnsNull()
		{
			// Arrange
			var fixture = Create();

			// Act
			var result = fixture.FetchNextJob();

			// Assert
			result.Should().BeNull();
		}

		[Fact]
		public void FetchNextJob_Commit()
		{
			// Arrange
			var fixture = Create();
			var model = new DelayedJob
			{
				Data = "data",
				Due = null
			};
			fixture.StoreJob(model);

			// Act
			var result = fixture.FetchNextJob();
			result.RemoveFromQueue();
			result.Dispose();

			// Assert
			fixture.FetchNextJob().Should().BeNull();
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
		public void FetchNextDelayedJob_NoJobs_ReturnsNull()
		{
			// Arrange
			var fixture = Create();

			// Act
			var result = fixture.FetchNextDelayedJob();

			// Assert
			result.Should().BeNull();
		}

		[Fact]
		public void FetchNextDelayedJob_Commit()
		{
			// Arrange
			var fixture = Create();
			var model = new DelayedJob
			{
				Data = "data",
				Due = DateTime.MinValue
			};
			fixture.StoreJob(model);

			// Act
			var result = fixture.FetchNextDelayedJob();
			result.RemoveFromQueue();
			result.Dispose();

			// Assert
			fixture.FetchNextJob().Should().BeNull();
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
		public void GetCronJobs()
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
			fixture.StoreJob(model1);
			fixture.StoreJob(model2);

			// Act
			var result = fixture.GetCronJobs();

			// Assert
			result.Should().HaveCount(2);
		}

		[Fact]
		public void GetCronJobByName()
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
			fixture.StoreJob(model1);
			fixture.StoreJob(model2);

			// Act
			var result = fixture.GetCronJobByName("2");

			// Assert
			result.Should().NotBeNull();
		}

		private SqlServerStorageConnection Create()
		{
			var services = new ServiceCollection();
			services.AddJobsSqlServer();
			services.AddOptions();
			services.AddLogging();
			services.Configure<SqlServerOptions>(
				options => options.ConnectionString = ConnectionUtil.GetConnectionString());
			var provider = services.BuildServiceProvider();
			return provider.GetService<IStorage>().GetConnection() as SqlServerStorageConnection;
		}
	}
}
