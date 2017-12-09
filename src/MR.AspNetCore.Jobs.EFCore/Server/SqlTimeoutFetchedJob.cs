using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace MR.AspNetCore.Jobs.Server
{
	public class SqlTimeoutFetchedJob : IFetchedJob
	{
		private bool _disposed;
		private bool _removedFromQueue;
		private bool _requeued;
		private readonly object _lock = new object();
		private readonly Timer _timer;

		private readonly IServiceProvider _services;

		public SqlTimeoutFetchedJob(
			IServiceProvider services,
			int id,
			int jobId)
		{
			_services = services;

			Id = id;
			JobId = jobId;

			var keepAliveInterval = TimeSpan.FromSeconds(TimeSpan.FromMinutes(1).TotalSeconds / 5);
			_timer = new Timer(ExecuteKeepAliveQuery, null, keepAliveInterval, keepAliveInterval);
		}

		public int Id { get; }

		public int JobId { get; }

		public Task RemoveFromQueueAsync()
		{
			lock (_lock)
			{
				using (var services = _services.CreateScope())
				{
					var storageConnection = GetStorageConnection(services.ServiceProvider);
					var connection = storageConnection.GetDbConnection();

					connection.Execute(
						CreateRemoveFromQueueQuery(storageConnection),
						new { id = Id });

					_removedFromQueue = true;
				}
			}

			return Task.CompletedTask;
		}

		protected virtual string CreateRemoveFromQueueQuery(EFCoreStorageConnection storageConnection)
		{
			return $"DELETE FROM {storageConnection.BaseOptions.Schema}.JobQueue WHERE Id = @id";
		}

		public Task RequeueAsync()
		{
			lock (_lock)
			{
				using (var services = _services.CreateScope())
				{
					var storageConnection = GetStorageConnection(services.ServiceProvider);
					var connection = storageConnection.GetDbConnection();

					connection.Execute(
						CreateRequeueQuery(storageConnection),
						new { id = Id });

					_requeued = true;
				}
			}

			return Task.CompletedTask;
		}

		protected virtual string CreateRequeueQuery(EFCoreStorageConnection storageConnection)
		{
			return $"UPDATE {storageConnection.BaseOptions.Schema}.JobQueue SET FetchedAt = NULL WHERE Id = @id";
		}

		public void Dispose()
		{
			if (_disposed) return;
			_disposed = true;

			_timer?.Dispose();

			lock (_lock)
			{
				if (!_removedFromQueue && !_requeued)
				{
					RequeueAsync().GetAwaiter().GetResult();
				}
			}
		}

		private void ExecuteKeepAliveQuery(object obj)
		{
			lock (_lock)
			{
				if (_requeued || _removedFromQueue) return;

				using (var services = _services.CreateScope())
				{
					var storageConnection = GetStorageConnection(services.ServiceProvider);
					var connection = storageConnection.GetDbConnection();

					connection.Execute(
						$"UPDATE {storageConnection.BaseOptions.Schema}.JobQueue SET FetchedAt = @date WHERE Id = @id",
						new { id = Id, date = DateTime.UtcNow });
				}
			}
		}

		private EFCoreStorageConnection GetStorageConnection(IServiceProvider services) =>
			services.GetService<IStorageConnection>() as EFCoreStorageConnection;
	}
}
