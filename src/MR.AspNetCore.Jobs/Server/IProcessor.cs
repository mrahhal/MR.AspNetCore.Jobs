using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MR.AspNetCore.Jobs.Server
{
	public interface IProcessor
	{
		void Process(ProcessingContext context);
	}

	public static class ProcessorExtensions
	{
		public static Task CreateTask(this IProcessor processor, ProcessingContext context, ILogger logger)
		{
			return Task.Factory.StartNew(() =>
				{
					TrySetThreadName(processor.ToString());

					try
					{
						processor.Process(context);
					}
					catch
					{
						logger.LogError(
							$"An error occured while executing a job processor: {processor.ToString()}");
					}
				},
				TaskCreationOptions.LongRunning);
		}

		private static void TrySetThreadName(string name)
		{
#if NET451
			try
			{
				System.Threading.Thread.CurrentThread.Name = name;
			}
			catch (System.InvalidOperationException)
			{
			}
#endif
		}
	}
}
