using System.Threading.Tasks;

namespace MR.AspNetCore.Jobs
{
	/// <summary>
	/// Represents a persisted storage.
	/// </summary>
	public interface IStorage
	{
		/// <summary>
		/// Initializes the storage.
		/// </summary>
		Task InitializeAsync();

		/// <summary>
		/// Returns an <see cref="IStorageConnection"/> that is connected to this storage.
		/// </summary>
		IStorageConnection GetConnection();
	}
}
