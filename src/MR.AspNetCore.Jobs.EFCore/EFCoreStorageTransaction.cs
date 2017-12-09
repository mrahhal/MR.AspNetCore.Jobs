using System;
using System.Threading.Tasks;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs
{
	public abstract class EFCoreStorageTransaction<TContext, TOptions> : IStorageTransaction, IDisposable
		where TContext : EFCoreJobsDbContext
		where TOptions : EFCoreOptions
	{
		public EFCoreStorageTransaction(EFCoreStorageConnection<TContext, TOptions> connection)
		{
			Connection = connection;
		}

		protected EFCoreStorageConnection<TContext, TOptions> Connection { get; }

		public virtual void UpdateJob(Job job)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));

			job.Updated = DateTime.UtcNow;

			// NOOP. EF will detect changes.
		}

		public virtual void EnqueueJob(Job job)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));

			Connection.Context.Add(new JobQueue
			{
				JobId = job.Id
			});
		}

		public virtual Task CommitAsync()
		{
			return Connection.Context.SaveChangesAsync();
		}

		public virtual void Dispose()
		{
		}
	}
}
