using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerStorageTransaction : IStorageTransaction, IDisposable
	{
		private SqlServerStorage _storage;

		private readonly Queue<Func<DbConnection, DbTransaction, Task>> _commandQueue
			= new Queue<Func<DbConnection, DbTransaction, Task>>();

		public SqlServerStorageTransaction(SqlServerStorage storage)
		{
			_storage = storage;
		}

		public void UpdateJob(Job job)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));

			var sql = @"
				UPDATE [Jobs].Jobs
				SET Due = @due, ExpiresAt = @expiresAt, Retries = @retries, StateName = @stateName
				WHERE Id = @id";

			QueueCommand((connection, transaction) =>
			{
				return connection.ExecuteAsync(sql, new
				{
					id = job.Id,
					due = job.Due,
					expiresAt = job.ExpiresAt,
					retries = job.Retries,
					stateName = job.StateName
				}, transaction);
			});
		}

		public void EnqueueJob(int id)
		{
			var sql = @"
				INSERT INTO [Jobs].JobQueue
				(JobId) VALUES (@jobId)";

			QueueCommand((connection, transaction) =>
			{
				return connection.ExecuteAsync(sql, new
				{
					jobId = id
				}, transaction);
			});
		}

		public Task CommitAsync()
		{
			return _storage.UseTransactionAsync(async (connection, transaction) =>
			{
				foreach (var command in _commandQueue)
				{
					await command(connection, transaction);
				}
			});
		}

		public void Dispose()
		{
		}

		internal void QueueCommand(Func<DbConnection, DbTransaction, Task> func)
		{
			_commandQueue.Enqueue(func);
		}
	}
}
