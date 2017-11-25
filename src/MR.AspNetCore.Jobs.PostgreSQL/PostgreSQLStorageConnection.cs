using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server.States;

namespace MR.AspNetCore.Jobs
{
	public class PostgreSQLStorageConnection : EFCoreStorageConnection<JobsDbContext, PostgreSQLOptions>
	{
		public PostgreSQLStorageConnection(
			JobsDbContext context,
			PostgreSQLOptions options)
			: base(context, options)
		{
		}

		public override IStorageTransaction CreateTransaction()
		{
			return new PostgreSQLStorageTransaction(this);
		}

		protected override string CreateFetchNextJobQuery()
		{
			var table = nameof(EFCoreJobsDbContext.JobQueue);
			return $@"
DELETE
FROM [{Options.Schema}].[{table}] --WITH (readpast, updlock, rowlock)
RETURNING {table}.JobId
LIMIT 1";
		}

		protected override string CreateGetNextJobToBeEnqueuedQuery()
		{
			return $@"
SELECT *
FROM [{Options.Schema}].[{nameof(EFCoreJobsDbContext.Jobs)}] --WITH (readpast)
WHERE (Due IS NULL OR Due < GETUTCDATE()) AND StateName = '{ScheduledState.StateName}'
LIMIT 1";
		}
	}
}
