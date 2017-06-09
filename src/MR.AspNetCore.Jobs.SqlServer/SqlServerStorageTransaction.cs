using System;
using System.Threading.Tasks;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerStorageTransaction : IStorageTransaction, IDisposable
	{
		private SqlServerStorageConnection _connection;

		public SqlServerStorageTransaction(SqlServerStorageConnection connection)
		{
			_connection = connection;
		}

		public void UpdateJob(Job job)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));

			// NOOP. EF will detect changes.
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
			return _connection.Context.SaveChangesAsync();
		}

		public void Dispose()
		{
		}
	}
}
