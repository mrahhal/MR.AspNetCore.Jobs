using System;

namespace MR.AspNetCore.Jobs.Server
{
	public class PostgresSQLTimeoutFetchedJob : SqlTimeoutFetchedJob
	{
		public PostgresSQLTimeoutFetchedJob(
			IServiceProvider services,
			int id,
			int jobId)
			: base(services, id, jobId)
		{
		}

		protected override string CreateRemoveFromQueueQuery(EFCoreStorageConnection storageConnection)
		{
			return $@"DELETE FROM ""{storageConnection.BaseOptions.Schema}"".""JobQueue"" WHERE ""Id"" = @id";
		}

		protected override string CreateRequeueQuery(EFCoreStorageConnection storageConnection)
		{
			return $@"UPDATE ""{storageConnection.BaseOptions.Schema}"".""JobQueue"" SET ""FetchedAt"" = NULL WHERE ""Id"" = @id";
		}
	}
}
