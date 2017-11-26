using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore.Storage;

namespace MR.AspNetCore.Jobs.Server
{
	public class SqlTransactionFetchedJob : IFetchedJob
	{
		private IDbConnection _connection;
		private IDbContextTransaction _transaction;
		private readonly Timer _timer;
		private readonly object _lock = new object();

		public SqlTransactionFetchedJob(
			int jobId,
			IDbConnection connection,
			IDbContextTransaction transaction)
		{
			JobId = jobId;
			_connection = connection;
			_transaction = transaction;
			if (TransactionCanTerminate)
			{
				_timer = new Timer(ExecuteKeepAliveQuery, null, KeepAliveInterval, KeepAliveInterval);
			}
		}

		public virtual bool TransactionCanTerminate => true;

		public virtual TimeSpan KeepAliveInterval => TimeSpan.FromMinutes(1);

		public int JobId { get; }

		public Task RemoveFromQueueAsync()
		{
			lock (_lock)
			{
				_transaction.Commit();
			}
			return Task.CompletedTask;
		}

		public Task RequeueAsync()
		{
			lock (_lock)
			{
				_transaction.Rollback();
			}
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			lock (_lock)
			{
				_timer?.Dispose();
				_transaction.Dispose();
				_connection = null;
			}
		}

		private void ExecuteKeepAliveQuery(object obj)
		{
			lock (_lock)
			{
				try
				{
					_connection?.Execute("SELECT 1;", transaction: _transaction.GetDbTransaction());
				}
				catch
				{
				}
			}
		}
	}
}
