using System.Data;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs.Server
{
	public class SqlServerFetchedJob : IFetchedJob
	{
		private SqlServerStorage _storage;
		private IDbConnection _connection;
		private IDbTransaction _transaction;

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
		}

		public DelayedJob Job { get; }

		public void RemoveFromQueue()
		{
			_transaction.Commit();
		}

		public void Requeue()
		{
			_transaction.Rollback();
		}

		public void Dispose()
		{
			_transaction.Dispose();
			_storage.ReleaseConnection(_connection);
			_connection = null;
		}
	}
}
