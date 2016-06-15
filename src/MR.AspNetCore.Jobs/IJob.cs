using System.Threading.Tasks;

namespace MR.AspNetCore.Jobs
{
	public interface IJob
	{
		Task ExecuteAsync();
	}
}
