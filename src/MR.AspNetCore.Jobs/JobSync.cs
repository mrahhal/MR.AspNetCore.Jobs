using System.Threading.Tasks;

namespace MR.AspNetCore.Jobs
{
	/// <summary>
	/// An <see cref="IJob"/> that executes synchronously.
	/// </summary>
	public abstract class JobSync : IJob
	{
		public Task ExecuteAsync()
		{
			Execute();
			return Task.CompletedTask;
		}

		/// <summary>
		/// Execute the job.
		/// </summary>
		public abstract void Execute();
	}
}
