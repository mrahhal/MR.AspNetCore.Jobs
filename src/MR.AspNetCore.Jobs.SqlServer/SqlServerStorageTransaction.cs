using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerStorageTransaction : EFCoreStorageTransaction<JobsDbContext, SqlServerOptions>
	{
		public SqlServerStorageTransaction(SqlServerStorageConnection connection)
			: base(connection)
		{
		}
	}
}
