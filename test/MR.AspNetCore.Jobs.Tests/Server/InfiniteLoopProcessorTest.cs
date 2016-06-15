using Xunit;

namespace MR.AspNetCore.Jobs.Server
{
	public class InfiniteLoopProcessorTest
	{
		[Fact]
		public void Process_ThrowingProcessingCanceledException_Returns()
		{
			// Arrange
			var inner = new ThrowsProcessingCanceledExceptionProcessor();
			var p = new InfiniteLoopProcessor(inner);
			var context = new ProcessingContext();

			// Act
			p.Process(context);
		}

		private class ThrowsProcessingCanceledExceptionProcessor : IProcessor
		{
			public void Process(ProcessingContext context)
			{
				throw new ProcessingCanceledException();
			}
		}
	}
}
