using System;
using System.Threading.Tasks;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs
{
	public interface IStorageTransaction : IDisposable
	{
		// Updates: Due, ExpiresAt, Retries, StateName
		void UpdateJob(Job job);

		void EnqueueJob(int id);

		Task CommitAsync();
	}
}
