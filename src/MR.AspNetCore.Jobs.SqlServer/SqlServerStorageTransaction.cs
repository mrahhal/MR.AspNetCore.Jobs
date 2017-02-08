using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerStorageTransaction : IStorageTransaction, IDisposable
	{
		private SqlServerStorageConnection _connection;
		private IDbContextTransaction _transaction;

		public SqlServerStorageTransaction(SqlServerStorageConnection connection)
		{
			_connection = connection;
			_transaction = connection.Context.Database.BeginTransaction();
		}

		public void UpdateJob(Job job)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));
			_connection.Context.Update(job);
		}

		public void EnqueueJob(Job job)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));
			_connection.Context.Add(new JobQueue
			{
				JobId = job.Id
			});
		}

		public Task CommitAsync()
		{
			_transaction.Commit();
			return _connection.Context.SaveChangesAsync();
		}

		public void Dispose()
		{
		}
	}
}
