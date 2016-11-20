using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server;
using MR.AspNetCore.Jobs.Server.States;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerStorageConnection : IStorageConnection
	{
		internal SqlServerStorage _storage;

		public SqlServerStorageConnection(SqlServerStorage storage)
		{
			_storage = storage;
		}

		public async Task StoreJobAsync(Job job)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));
			job.Due = NormalizeDateTime(job.Due);

			var sql = @"
				INSERT INTO [Jobs].Jobs
				(Data, Added, Due, ExpiresAt, Retries, StateName)
				VALUES
				(@Data, @Added, @Due, @ExpiresAt, @Retries, @StateName)

				SELECT CAST(SCOPE_IDENTITY() as int)";

			var id = await _storage.UseConnectionAsync(async connection =>
			{
				return (await connection.QueryAsync<int>(sql, new
				{
					job.Id,
					job.Data,
					job.Added,
					job.Due,
					job.ExpiresAt,
					job.Retries,
					job.StateName
				})).Single();
			});
			job.Id = id;
		}

		public Task<Job> GetJobAsync(int id)
		{
			var sql = @"
				SELECT * FROM [Jobs].Jobs
				WHERE Id = @id";

			return _storage.UseConnectionAsync(async connection =>
			{
				return (await connection.QueryAsync<Job>(sql, new
				{
					id
				})).FirstOrDefault();
			});
		}

		public Task<IFetchedJob> FetchNextJobAsync()
		{
			var sql = @"
				DELETE TOP (1)
				FROM [Jobs].JobQueue WITH (readpast, updlock, rowlock)
				OUTPUT DELETED.JobId";

			return FetchNextDelayedJobCoreAsync(sql);
		}

		public Task<Job> GetNextJobToBeEnqueuedAsync()
		{
			var sql = $@"
				SELECT TOP (1) *
				FROM [Jobs].Jobs WITH (readpast)
				WHERE (Due IS NULL OR Due < GETUTCDATE()) AND StateName = '{ScheduledState.StateName}'";

			return _storage.UseConnectionAsync(async connection =>
			{
				return (await connection.QueryAsync<Job>(sql)).FirstOrDefault();
			});
		}

		public Task StoreCronJobAsync(CronJob job)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));

			var sql = @"
				INSERT INTO [Jobs].CronJobs
				(Id, Name, TypeName, Cron, LastRun)
				VALUES
				(@Id, @Name, @TypeName, @Cron, @LastRun)";

			return _storage.UseConnectionAsync(connection =>
			{
				return connection.ExecuteAsync(sql, new
				{
					job.Id,
					job.Name,
					job.TypeName,
					job.Cron,
					LastRun = NormalizeDateTime(job.LastRun)
				});
			});
		}

		public Task UpdateCronJobAsync(CronJob job)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));

			var sql = @"
				UPDATE [Jobs].CronJobs
				SET TypeName = @TypeName, Cron = @Cron, LastRun = @LastRun
				WHERE Id = @Id";

			return _storage.UseConnectionAsync(connection =>
			{
				return connection.ExecuteAsync(sql, new
				{
					job.Id,
					job.TypeName,
					job.Cron,
					job.LastRun
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

		public IStorageTransaction CreateTransaction()
		{
			return new SqlServerStorageTransaction(_storage);
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
			FetchedJob fetchedJob = null;
			var connection = _storage.CreateAndOpenConnection();
			var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

			try
			{
				fetchedJob =
					(await connection.QueryAsync<FetchedJob>(sql, args, transaction))
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
				fetchedJob.JobId,
				_storage,
				connection,
				transaction);
		}
	}
}
