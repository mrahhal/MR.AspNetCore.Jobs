using System.Threading.Tasks;

namespace MR.AspNetCore.Jobs
{
	public abstract class JobSync : IJob
	{
		public Task ExecuteAsync()
		{
			Execute();
			return Task.FromResult(0);
		}

		public abstract void Execute();
	}
}
