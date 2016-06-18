using System.Threading.Tasks;

namespace MR.AspNetCore.Jobs
{
	public interface IBootstrapper
	{
		Task BootstrapAsync();
	}
}
