using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerStorageConnection : IStorageConnection
	{
		internal SqlServerStorage _storage;

		public SqlServerStorageConnection(SqlServerStorage storage)
		{
			_storage = storage;
		}

		public async Task StoreDelayedJobAsync(DelayedJob job, DateTime? due)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));
			due = NormalizeDateTime(due);

			var sql = @"
				INSERT INTO [Jobs].DelayedJobs
				(Id, Data, Added)
				VALUES
				(@id, @data, @added)

				INSERT INTO [Jobs].DelayedJobDue
				(DelayedJobId, Due)
				VALUES
				(@id, @due)";

			using (var connection = _storage.CreateAndOpenConnection())
			using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
			{
				await connection.ExecuteAsync(sql, new
				{
					id = job.Id,
					data = job.Data,
					added = job.Added,
					due
				}, transaction);
				transaction.Commit();
			}
		}

		public Task<IFetchedJob> FetchNextDelayedJobAsync()
		{
			var sql = @"
				DELETE TOP (1) dj OUTPUT DELETED.*
				FROM [Jobs].DelayedJobs dj
				WITH (readpast, updlock, rowlock)
				LEFT OUTER JOIN [Jobs].DelayedJobDue djd ON djd.DelayedJobId = dj.Id
				WHERE djd.Due IS NULL OR djd.Due < GETUTCDATE()";

			return FetchNextDelayedJobCoreAsync(sql);
		}

		public Task<string> GetDelayedJobParameterAsync(string id, string name)
		{
			if (id == null) throw new ArgumentNullException(nameof(id));
			if (name == null) throw new ArgumentNullException(nameof(name));

			return _storage.UseConnectionAsync(async connection =>
			{
				return (await connection.QueryAsync<string>(@"
					SELECT Value FROM [Jobs].DelayedJobParameters WITH (readcommittedlock)
					WHERE DelayedJobId = @id and Name = @name",
					new { id = id, name = name })).SingleOrDefault();
			});
		}

		public Task SetDelayedJobParameterAsync(string id, string name, string value)
		{
			if (id == null) throw new ArgumentNullException(nameof(id));
			if (name == null) throw new ArgumentNullException(nameof(name));

			return _storage.UseConnectionAsync(connection =>
			{
				return connection.ExecuteAsync(@"
					MERGE [Jobs].DelayedJobParameters WITH (holdlock) AS Target
					USING (VALUES (@jobId, @name, @value)) AS Source (JobId, Name, Value)
					ON Target.DelayedJobId = Source.DelayedJobId AND Target.Name = Source.Name
					WHEN MATCHED THEN UPDATE SET Value = Source.Value
					WHEN NOT MATCHED THEN INSERT (JobId, Name, Value)
					VALUES (Source.DelayedJobId, Source.Name, Source.Value)",
					new { jobId = id, name, value });
			});
		}

		public Task SetDelayedJobDue(string id, DateTime? due)
		{
			if (id == null) throw new ArgumentNullException(nameof(id));

			due = NormalizeDateTime(due);

			var sql = @"
				UPDATE [Jobs].DelayedJobDue
				SET Due = @due
				WHERE DelayedJobId = @jobId";

			return _storage.UseConnectionAsync(connection =>
			{
				return connection.ExecuteAsync(sql, new
				{
					jobId = id,
					due
				});
			});
		}

		public Task StoreCronJobAsync(CronJob job)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));

			var sql = @"
				INSERT INTO [Jobs].CronJobs
				(Id, Name, TypeName, Cron, LastRun)
				VALUES
				(@id, @name, @typeName, @cron, @lastRun)";

			return _storage.UseConnectionAsync(connection =>
			{
				return connection.ExecuteAsync(sql, new
				{
					id = job.Id,
					name = job.Name,
					typeName = job.TypeName,
					cron = job.Cron,
					lastRun = NormalizeDateTime(job.LastRun)
				});
			});
		}

		public Task UpdateCronJobAsync(CronJob job)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));

			var sql = @"
				UPDATE [Jobs].CronJobs
				SET TypeName = @typeName, Cron = @cron, LastRun = @lastRun
				WHERE Id = @id";

			return _storage.UseConnectionAsync(connection =>
			{
				return connection.ExecuteAsync(sql, new
				{
					id = job.Id,
					typeName = job.TypeName,
					cron = job.Cron,
					lastRun = job.LastRun
				});
			});
		}

		public Task<CronJob[]> GetCronJobsAsync()
		{
			var sql = @"
				SELECT * FROM [Jobs].CronJobs";

			return _storage.UseConnectionAsync(async connection =>
			{
				return
					(await connection.QueryAsync<CronJob>(sql))
					.ToArray();
			});
		}

		public Task<CronJob> GetCronJobByNameAsync(string name)
		{
			var sql = @"
				SELECT * FROM [Jobs].CronJobs
				WHERE Name = @name";

			return _storage.UseConnectionAsync(async connection =>
			{
				return
					(await connection.QueryAsync<CronJob>(sql, new { name }))
					.FirstOrDefault();
			});
		}

		public Task RemoveCronJobAsync(string name)
		{
			var sql = @"
				DELETE FROM [Jobs].CronJobs
				WHERE Name = @name";

			return _storage.UseConnectionAsync(connection =>
			{
				return connection.ExecuteAsync(sql, new { name });
			});
		}

		public void Dispose()
		{
		}

		private DateTime? NormalizeDateTime(DateTime? dateTime)
		{
			if (!dateTime.HasValue) return dateTime;
			if (dateTime == DateTime.MinValue)
			{
				return new DateTime(1754, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			}
			return dateTime;
		}

		private async Task<IFetchedJob> FetchNextDelayedJobCoreAsync(string sql, object args = null)
		{
			DelayedJob fetchedJob = null;
			var connection = _storage.CreateAndOpenConnection();
			var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

			try
			{
				fetchedJob =
					(await connection.QueryAsync<DelayedJob>(sql, args, transaction))
					.FirstOrDefault();
			}
			catch (SqlException)
			{
				transaction.Dispose();
				_storage.ReleaseConnection(connection);
				throw;
			}

			if (fetchedJob == null)
			{
				transaction.Rollback();
				transaction.Dispose();
				_storage.ReleaseConnection(connection);
				return null;
			}

			return new SqlServerFetchedJob(
				fetchedJob,
				_storage,
				connection,
				transaction);
		}
	}
}
