using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server;
using Xunit;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerBootstrapperTest
	{
		private Mock<IProcessingServer> _mockProcessingServer;
		private Mock<IStorage> _mockStorage;
		private ServiceCollection _services;
		private Mock<IHostApplicationLifetime> _mockApplicationLifetime;
		private Mock<IStorageConnection> _mockStorageConnection;

		public SqlServerBootstrapperTest()
		{
			_services = new ServiceCollection();
			_services.AddLogging();
			_services.AddSingleton(new JobsOptions());
			_mockProcessingServer = new Mock<IProcessingServer>();
			_services.AddSingleton(_mockProcessingServer.Object);
			_mockStorage = new Mock<IStorage>();
			_mockStorageConnection = new Mock<IStorageConnection>();
			_services.AddSingleton(_mockStorage.Object);
			_services.AddSingleton(_mockStorageConnection.Object);
			_mockApplicationLifetime = new Mock<IHostApplicationLifetime>();
			_services.AddSingleton(_mockApplicationLifetime.Object);
			_services.AddTransient<SqlServerBootstrapper>();
		}

		[Fact]
		public async Task Bootstrap_CallsStorage_Initialize()
		{
			// Arrange
			_services.AddSingleton(new JobsOptions());
			var provider = _services.BuildServiceProvider();
			var bootstrapper = provider.GetService<SqlServerBootstrapper>();

			// Act
			await bootstrapper.BootstrapAsync();

			// Assert
			_mockStorage.Verify(s => s.InitializeAsync(It.IsAny<CancellationToken>()));
		}

		[Fact]
		public async Task Bootstrap_CallsProcessingServer_Start()
		{
			// Arrange
			_services.AddSingleton(new JobsOptions());
			var provider = _services.BuildServiceProvider();
			var bootstrapper = provider.GetService<SqlServerBootstrapper>();

			// Act
			await bootstrapper.BootstrapAsync();

			// Assert
			_mockProcessingServer.Verify(s => s.Start());
		}

		[Fact]
		public async Task Bootstrap_UpdatesCronJobs()
		{
			// Arrange
			_services.AddSingleton(
				CreateOptionsWithRegistry(new BazCronJobRegistry()));
			_mockStorageConnection.Setup(m => m.GetCronJobsAsync())
				.ReturnsAsync(GetCronJobsFromRegistry(new FooCronJobRegistry()));
			var provider = _services.BuildServiceProvider();
			var bootstrapper = provider.GetService<SqlServerBootstrapper>();

			// Act
			await bootstrapper.BootstrapAsync();

			// Assert
			_mockStorageConnection
				.Verify(m => m.UpdateCronJobAsync(It.Is<CronJob>(j => j.Name == nameof(FooJob))), Times.Once());
		}

		[Fact]
		public async Task Bootstrap_RemovesOldCronJobs()
		{
			// Arrange
			_services.AddSingleton(
				CreateOptionsWithRegistry(new BarCronJobRegistry()));
			_mockStorageConnection.Setup(m => m.GetCronJobsAsync())
				.ReturnsAsync(GetCronJobsFromRegistry(new FooCronJobRegistry()));
			var provider = _services.BuildServiceProvider();
			var bootstrapper = provider.GetService<SqlServerBootstrapper>();

			// Act
			await bootstrapper.BootstrapAsync();

			// Assert
			_mockStorageConnection.Verify(m => m.RemoveCronJobAsync(nameof(FooJob)), Times.Once());
		}

		private CronJob[] GetCronJobsFromRegistry(CronJobRegistry registry)
		{
			return registry.Build().Select(j => new CronJob()
			{
				Cron = j.Cron,
				Name = j.Name,
				TypeName = j.JobType.AssemblyQualifiedName
			}).ToArray();
		}

		private JobsOptions CreateOptionsWithRegistry(CronJobRegistry registry)
		{
			var options = new JobsOptions();
			options.UseCronJobRegistry(registry);
			return options;
		}

		private class FooCronJobRegistry : CronJobRegistry
		{
			public FooCronJobRegistry()
			{
				RegisterJob<FooJob>(nameof(FooJob), Cron.Daily());
			}
		}

		private class BarCronJobRegistry : CronJobRegistry
		{
			public BarCronJobRegistry()
			{
			}
		}

		private class BazCronJobRegistry : CronJobRegistry
		{
			public BazCronJobRegistry()
			{
				RegisterJob<FooJob>(nameof(FooJob), Cron.Monthly());
			}
		}

		private class FooJob : IJob
		{
			public Task ExecuteAsync()
			{
				return Task.CompletedTask;
			}
		}
	}
}
