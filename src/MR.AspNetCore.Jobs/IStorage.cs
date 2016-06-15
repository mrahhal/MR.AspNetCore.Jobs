namespace MR.AspNetCore.Jobs
{
	public interface IStorage
	{
		void Initialize();
		IStorageConnection GetConnection();
	}
}
