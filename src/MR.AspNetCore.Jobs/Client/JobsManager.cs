using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server;

namespace MR.AspNetCore.Jobs.Client
{
	public class JobsManager : IJobsManager
	{
		private JobsOptions _options;
		private IStorage _storage;
		private IProcessingServer _server;

		public JobsManager(
			JobsOptions options,
			IStorage storage,
			IProcessingServer server)
		{
			_options = options;
			_storage = storage;
			_server = server;
		}

		public async Task EnqueueAsync(Expression<Action> methodCall)
		{
			if (methodCall == null) throw new ArgumentNullException(nameof(methodCall));

			var method = MethodInvocation.FromExpression(methodCall);
			await EnqueueCore(null, method);
			_server.Pulse(PulseKind.BackgroundJobEnqueued);
		}

		public async Task EnqueueAsync<T>(Expression<Action<T>> methodCall)
		{
			if (methodCall == null) throw new ArgumentNullException(nameof(methodCall));

			var method = MethodInvocation.FromExpression(methodCall);
			await EnqueueCore(null, method);
			_server.Pulse(PulseKind.BackgroundJobEnqueued);
		}

		public Task EnqueueAsync(Expression<Action> methodCall, DateTimeOffset due)
		{
			if (methodCall == null) throw new ArgumentNullException(nameof(methodCall));

			var method = MethodInvocation.FromExpression(methodCall);
			return EnqueueCore(due.UtcDateTime, method);
		}

		public Task EnqueueAsync<T>(Expression<Action<T>> methodCall, DateTimeOffset due)
		{
			if (methodCall == null) throw new ArgumentNullException(nameof(methodCall));

			var method = MethodInvocation.FromExpression(methodCall);
			return EnqueueCore(due.UtcDateTime, method);
		}

		private async Task EnqueueCore(DateTime? due, MethodInvocation method)
		{
			var data = InvocationData.Serialize(method);

			var job = new DelayedJob()
			{
				Due = due,
				Data = Helper.ToJson(data)
			};

			using (var connection = _storage.GetConnection())
			{
				await connection.StoreJobAsync(job);
			}
		}
	}
}
