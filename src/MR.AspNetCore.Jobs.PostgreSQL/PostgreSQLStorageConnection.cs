using System;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server;
using MR.AspNetCore.Jobs.Server.States;

namespace MR.AspNetCore.Jobs
{
	public class PostgreSQLStorageConnection : EFCoreStorageConnection<JobsDbContext, PostgreSQLOptions>
	{
		public PostgreSQLStorageConnection(
			JobsDbContext context,
			PostgreSQLOptions options,
			IServiceProvider services)
			: base(context, options, services)
		{
		}

		protected override bool UseTransactionFetchedJob => false;

		public override IStorageTransaction CreateTransaction()
		{
			return new PostgreSQLStorageTransaction(this);
		}

		protected override string CreateFetchNextJobQuery()
		{
			var table = nameof(EFCoreJobsDbContext.JobQueue);
			var timeoutSeconds = TimeSpan.FromMinutes(1).Negate().TotalSeconds;

			return $@"
UPDATE ""{Options.Schema}"".""{table}""
SET ""FetchedAt"" = NOW() AT TIME ZONE 'UTC'
WHERE CTID IN (
	SELECT CTID FROM ""{Options.Schema}"".""{table}""
	WHERE ""FetchedAt"" IS NULL OR ""FetchedAt"" < NOW() AT TIME ZONE 'UTC' + INTERVAL '{timeoutSeconds} SECONDS'
	LIMIT 1
)
RETURNING ""{table}"".""JobId""";
		}

		protected override string CreateGetNextJobToBeEnqueuedQuery()
		{
			return $@"
SELECT *
FROM ""{Options.Schema}"".""{nameof(EFCoreJobsDbContext.Jobs)}""
WHERE (Due IS NULL OR Due < NOW() AT TIME ZONE 'UTC') AND StateName = '{ScheduledState.StateName}'
LIMIT 1";
		}

		protected override IFetchedJob CreateSqlTimeoutFetchedJob(FetchedJob fetchedJob)
		{
			return new PostgresSQLTimeoutFetchedJob(
				Services,
				fetchedJob.Id,
				fetchedJob.JobId);
		}
	}
}
