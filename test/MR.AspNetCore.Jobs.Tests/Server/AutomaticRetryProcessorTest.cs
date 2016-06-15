using System;
using Moq;
using Xunit;

namespace MR.AspNetCore.Jobs.Server
{
	public class AutomaticRetryProcessorTest
	{
		[Fact]
		public void Process_Retries()
		{
			// Arrange
			var inner = new InnerProcessor(0);
			var p = new AutomaticRetryProcessor(inner);
			var context = new ProcessingContext();

			// Act
			p.Process(context);
		}

		[Fact]
		public void Process_Ends()
		{
			// Arrange
			var inner = new AlwaysThrowsProcessor();
			var p = new AutomaticRetryProcessor(inner);
			var context = new ProcessingContext();

			// Act + Assert
			Assert.Throws<Exception>(() => p.Process(context));
		}

		private class InnerProcessor : IProcessor
		{
			private int _throwsAt;
			private int _at;

			public InnerProcessor(int throwsAt)
			{
				_throwsAt = throwsAt;
			}

			public void Process(ProcessingContext context)
			{
				if (++_at == _throwsAt)
				{
					throw new Exception();
				}
			}
		}

		private class AlwaysThrowsProcessor : IProcessor
		{
			public void Process(ProcessingContext context)
			{
				throw new Exception();
			}
		}
	}
}
