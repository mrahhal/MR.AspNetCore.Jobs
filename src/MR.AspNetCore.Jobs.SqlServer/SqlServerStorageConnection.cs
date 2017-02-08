using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server;
using MR.AspNetCore.Jobs.Server.States;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerStorageConnection : IStorageConnection
	{
		private JobsDbContext _context;
		private SqlServerOptions _options;

		public SqlServerStorageConnection(
			JobsDbContext context,
			SqlServerOptions options)
		{
			_context = context;
			_options = options;
		}

		public JobsDbContext Context => _context;

		public SqlServerOptions Options => _options;

		public Task StoreJobAsync(Job job)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));
			job.Due = NormalizeDateTime(job.Due);

			_context.Add(job);
			return _context.SaveChangesAsync();
		}

		public Task<Job> GetJobAsync(int id)
		{
			return _context.Jobs.FirstOrDefaultAsync(j => j.Id == id);
		}

		public Task<IFetchedJob> FetchNextJobAsync()
		{
			var sql = $@"
				DELETE TOP (1)
				FROM [{_options.Schema}].[{nameof(JobsDbContext.JobQueue)}] WITH (readpast, updlock, rowlock)
				OUTPUT DELETED.JobId";

			return FetchNextDelayedJobCoreAsync(sql);
		}

		public async Task<Job> GetNextJobToBeEnqueuedAsync()
		{
			var sql = $@"
				SELECT TOP (1) *
				FROM [{_options.Schema}].[{nameof(JobsDbContext.Jobs)}] WITH (readpast)
				WHERE (Due IS NULL OR Due < GETUTCDATE()) AND StateName = '{ScheduledState.StateName}'";

			var connection = _context.GetDbConnection();
			return (await connection.QueryAsync<Job>(sql)).FirstOrDefault();
		}

		public Task StoreCronJobAsync(CronJob job)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));

			_context.Add(job);
			return _context.SaveChangesAsync();
		}

		public Task UpdateCronJobAsync(CronJob job)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));

			_context.Update(job);
			return _context.SaveChangesAsync();
		}

		public Task<CronJob[]> GetCronJobsAsync()
		{
			return _context.CronJobs.ToArrayAsync();
		}

		public async Task RemoveCronJobAsync(string name)
		{
			var cronJob = await _context.CronJobs.FirstOrDefaultAsync(j => j.Name == name);
			if (cronJob != null)
			{
				_context.Remove(cronJob);
				await _context.SaveChangesAsync();
			}
		}

		public IStorageTransaction CreateTransaction()
		{
			return new SqlServerStorageTransaction(this);
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
			var connection = _context.GetDbConnection();
			var transaction = _context.Database.CurrentTransaction;
			transaction = transaction ?? await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

			try
			{
				fetchedJob =
					(await connection.QueryAsync<FetchedJob>(sql, args, transaction.GetDbTransaction()))
					.FirstOrDefault();
			}
			catch (SqlException)
			{
				transaction.Dispose();
				throw;
			}

			if (fetchedJob == null)
			{
				transaction.Rollback();
				transaction.Dispose();
				return null;
			}

			return new SqlServerFetchedJob(
				fetchedJob.JobId,
				connection,
				transaction);
		}
	}
}
