using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MR.AspNetCore.Jobs.Server
{
	public class ProcessingServerTest
	{
		private Mock<IStorage> _mockStorage;
		private JobsOptions _options;
		private IServiceProvider _provider;

		public ProcessingServerTest()
		{
			_options = new JobsOptions()
			{
				PollingDelay = 0
			};
			_mockStorage = new Mock<IStorage>();

			var services = new ServiceCollection();
			services.AddTransient<ProcessingServer>();
			services.AddLogging();
			services.AddSingleton(_options);
			services.AddSingleton(_mockStorage.Object);
			_provider = services.BuildServiceProvider();
		}

		public ProcessingServer Create() => _provider.GetService<ProcessingServer>();
	}
}
