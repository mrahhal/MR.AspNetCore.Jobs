using System.Threading.Tasks;

namespace MR.AspNetCore.Jobs.Server
{
	public interface IProcessor
	{
		Task ProcessAsync(ProcessingContext context);
	}

	public interface IAdditionalProcessor : IProcessor
	{
	}
}
