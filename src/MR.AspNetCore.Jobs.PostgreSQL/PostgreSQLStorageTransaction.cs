using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs
{
	public class PostgreSQLStorageTransaction : EFCoreStorageTransaction<JobsDbContext, PostgreSQLOptions>
	{
		public PostgreSQLStorageTransaction(PostgreSQLStorageConnection connection)
			: base(connection)
		{
		}
	}
}
