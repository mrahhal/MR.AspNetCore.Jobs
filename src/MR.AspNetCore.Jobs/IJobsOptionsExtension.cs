using Microsoft.Extensions.DependencyInjection;

namespace MR.AspNetCore.Jobs
{
	public interface IJobsOptionsExtension
	{
		void AddServices(IServiceCollection services);
	}
}
