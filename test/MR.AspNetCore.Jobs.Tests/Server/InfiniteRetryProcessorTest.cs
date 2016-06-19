using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MR.AspNetCore.Jobs.Server
{
	public class InfiniteRetryProcessorTest
	{
		[Fact]
		public async Task Process_ThrowingProcessingCanceledException_Returns()
		{
			// Arrange
			var services = new ServiceCollection();
			services.AddLogging();
			var loggerFactory = services.BuildServiceProvider().GetService<ILoggerFactory>();
			var inner = new ThrowsProcessingCanceledExceptionProcessor();
			var p = new InfiniteRetryProcessor(inner, loggerFactory);
			var context = new ProcessingContext();

			// Act
			await p.ProcessAsync(context);
		}

		private class ThrowsProcessingCanceledExceptionProcessor : IProcessor
		{
			public Task ProcessAsync(ProcessingContext context)
			{
				throw new OperationCanceledException();
			}
		}
	}
}
