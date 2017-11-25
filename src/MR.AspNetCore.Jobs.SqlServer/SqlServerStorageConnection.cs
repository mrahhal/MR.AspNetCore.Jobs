using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server.States;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerStorageConnection : EFCoreStorageConnection<JobsDbContext, SqlServerOptions>
	{
		public SqlServerStorageConnection(
			JobsDbContext context,
			SqlServerOptions options)
			: base(context, options)
		{
		}

		public override IStorageTransaction CreateTransaction()
		{
			return new SqlServerStorageTransaction(this);
		}

		protected override string CreateFetchNextJobQuery()
		{
			return $@"
DELETE TOP (1)
FROM [{Options.Schema}].[{nameof(EFCoreJobsDbContext.JobQueue)}] WITH (readpast, updlock, rowlock)
OUTPUT DELETED.JobId";
		}

		protected override string CreateGetNextJobToBeEnqueuedQuery()
		{
			return $@"
SELECT TOP (1) *
FROM [{Options.Schema}].[{nameof(EFCoreJobsDbContext.Jobs)}] WITH (readpast)
WHERE (Due IS NULL OR Due < GETUTCDATE()) AND StateName = '{ScheduledState.StateName}'";
		}
	}
}
