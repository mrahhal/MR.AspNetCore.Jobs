using System.Threading.Tasks;

namespace MR.AspNetCore.Jobs
{
	public interface IStorage
	{
		Task InitializeAsync();
		IStorageConnection GetConnection();
	}
}
