using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using MR.AspNetCore.Jobs.Server;
using Xunit;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerBootstrapperTest : DatabaseTestHost
	{
		[Fact]
		public void Bootstrap_CallsStorageInitialize()
		{
			// Arrange
			var services = new ServiceCollection();
			services.AddJobsSqlServer();
			services.AddLogging();
			services.AddSingleton(new JobsOptions());
			services.AddSingleton(Mock.Of<IApplicationLifetime>());
			services.AddSingleton(Mock.Of<IProcessingServer>());
			var mockStorage = new Mock<IStorage>();
			services.Replace(ServiceDescriptor.Singleton(mockStorage.Object));
			var provider = services.BuildServiceProvider();

			// Act
			var bootstrapper = provider.GetService<IBootstrapper>();

			// Assert
			bootstrapper.Bootstrap();
			mockStorage.Verify(s => s.Initialize());
		}
	}
}
