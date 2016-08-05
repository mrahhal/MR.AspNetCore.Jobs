using System.Threading.Tasks;

namespace MR.AspNetCore.Jobs
{
	/// <summary>
	/// Represents a job that can be executed.
	/// </summary>
	public interface IJob
	{
		/// <summary>
		/// Executes the job.
		/// </summary>
		Task ExecuteAsync();
	}
}
