namespace MR.AspNetCore.Jobs.Server
{
	public class InfiniteLoopProcessor : IProcessor
	{
		private IProcessor _inner;

		public InfiniteLoopProcessor(IProcessor inner)
		{
			_inner = inner;
		}

		public override string ToString() => _inner.ToString();

		public void Process(ProcessingContext context)
		{
			try
			{
				while (!context.IsStopping)
				{
					_inner.Process(context);
				}
			}
			catch (ProcessingCanceledException)
			{
				return;
			}
		}
	}
}
