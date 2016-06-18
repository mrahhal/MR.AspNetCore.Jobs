using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MR.AspNetCore.Jobs.Server
{
	public interface IProcessor
	{
		Task ProcessAsync(ProcessingContext context);
	}
}
