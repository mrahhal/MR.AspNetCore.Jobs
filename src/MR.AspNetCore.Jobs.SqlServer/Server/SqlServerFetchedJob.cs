using System;
using System.Data;
using System.Threading;
using Dapper;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs.Server
{
	public class SqlServerFetchedJob : IFetchedJob
	{
		private SqlServerStorage _storage;
		private IDbConnection _connection;
		private IDbTransaction _transaction;
		private readonly Timer _timer;
		private static readonly TimeSpan KeepAliveInterval = TimeSpan.FromMinutes(1);
		private readonly object _lockObject = new object();

		public SqlServerFetchedJob(
			DelayedJob job,
			SqlServerStorage storage,
			IDbConnection connection,
			IDbTransaction transaction)
		{
			Job = job;
			_storage = storage;
			_connection = connection;
			_transaction = transaction;
			_timer = new Timer(ExecuteKeepAliveQuery, null, KeepAliveInterval, KeepAliveInterval);
		}

		public DelayedJob Job { get; }

		public void RemoveFromQueue()
		{
			lock (_lockObject)
			{
				_transaction.Commit();
			}
		}

		public void Requeue()
		{
			lock (_lockObject)
			{
				_transaction.Rollback();
			}
		}

		public void Dispose()
		{
			lock (_lockObject)
			{
				_timer?.Dispose();
				_transaction.Dispose();
				_storage.ReleaseConnection(_connection);
				_connection = null;
			}
		}

		private void ExecuteKeepAliveQuery(object obj)
		{
			lock (_lockObject)
			{
				try
				{
					_connection?.Execute("SELECT 1;", transaction: _transaction);
				}
				catch
				{
				}
			}
		}
	}
}
