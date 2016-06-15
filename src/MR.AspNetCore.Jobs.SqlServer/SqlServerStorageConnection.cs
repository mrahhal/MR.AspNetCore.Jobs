using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using MR.AspNetCore.Jobs.Client;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerStorageConnection : IStorageConnection
	{
		internal SqlServerStorage _storage;

		public SqlServerStorageConnection(SqlServerStorage storage)
		{
			_storage = storage;
		}

		public void StoreJob(DelayedJob job)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));

			var sql = @"
				INSERT INTO Jobs.DelayedJobs
				(Id, Data, Due)
				VALUES
				(@id, @data, @due)";

			_storage.UseConnection(connection =>
			{
				connection.Execute(sql, new
				{
					id = job.Id,
					data = job.Data,
					due = job.Due.HasValue ? NormalizeDateTime(job.Due.Value) : job.Due
				});
			});
		}

		public void StoreJob(CronJob job)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));

			var sql = @"
				INSERT INTO Jobs.CronJobs
				(Id, Name, TypeName, Cron, LastRun)
				VALUES
				(@id, @name, @typeName, @cron, @lastRun)";

			_storage.UseConnection(connection =>
			{
				connection.Execute(sql, new
				{
					id = job.Id,
					name = job.Name,
					typeName = job.TypeName,
					cron = job.Cron,
					lastRun = NormalizeDateTime(job.LastRun)
				});
			});
		}

		public void UpdateCronJob(CronJob job)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));

			var sql = @"
				UPDATE Jobs.CronJobs
				SET TypeName = @typeName, Cron = @cron, LastRun = @lastRun
				WHERE Id = @id";

			_storage.UseConnection(connection =>
			{
				connection.Execute(sql, new
				{
					id = job.Id,
					typeName = job.TypeName,
					cron = job.Cron,
					lastRun = job.LastRun
				});
			});
		}

		public IFetchedJob FetchNextJob()
		{
			var sql = @"
				DELETE TOP (1) from Jobs.DelayedJobs with (readpast, updlock, rowlock)
				output DELETED.*
				WHERE Due IS NULL";

			return FetchNextDelayedJobCore(sql);
		}

		public IFetchedJob FetchNextDelayedJob(DateTime from, DateTime to)
		{
			var sql = @"
				DELETE TOP (1) from Jobs.DelayedJobs with (readpast, updlock, rowlock)
				output DELETED.*
				WHERE Due >= @from AND Due < @to";

			return FetchNextDelayedJobCore(sql, new { from = NormalizeDateTime(from), to });
		}

		private DateTime NormalizeDateTime(DateTime from)
		{
			if (from == DateTime.MinValue)
			{
				return new DateTime(1754, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			}
			return from;
		}

		private IFetchedJob FetchNextDelayedJobCore(string sql, object args = null)
		{
			DelayedJob fetchedJob = null;
			var connection = _storage.CreateAndOpenConnection();
			var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

			try
			{
				fetchedJob = connection.Query<DelayedJob>(sql, args, transaction).FirstOrDefault();
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

		public CronJob[] GetCronJobs()
		{
			var sql = @"
				SELECT * FROM Jobs.CronJobs";

			return _storage.UseConnection(connection =>
			{
				return connection.Query<CronJob>(sql).ToArray();
			});
		}

		public CronJob GetCronJobByName(string name)
		{
			var sql = @"
				SELECT * FROM Jobs.CronJobs
				WHERE Name = @name";

			return _storage.UseConnection(connection =>
			{
				return connection.Query<CronJob>(sql, new
				{
					name
				}).FirstOrDefault();
			});
		}

		public void RemoveCronJob(string name)
		{
			var sql = @"
				DELETE FROM Jobs.CronJobs
				WHERE Name = @name";

			_storage.UseConnection(connection =>
			{
				connection.Execute(sql, new { name });
			});
		}

		public void Dispose()
		{
		}
	}
}
