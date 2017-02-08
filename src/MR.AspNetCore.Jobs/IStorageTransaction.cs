using System;
using System.Threading.Tasks;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs
{
	public interface IStorageTransaction : IDisposable
	{
		void UpdateJob(Job job);

		void EnqueueJob(Job job);

		Task CommitAsync();
	}
}
