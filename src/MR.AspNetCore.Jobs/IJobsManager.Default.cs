using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server;
using MR.AspNetCore.Jobs.Server.States;

namespace MR.AspNetCore.Jobs
{
	/// <summary>
	/// The default <see cref="IJobsManager"/>.
	/// </summary>
	public class JobsManager : IJobsManager
	{
		private JobsOptions _options;
		private IStorage _storage;
		private IStateChanger _stateChanger;
		private IProcessingServer _server;
		private IStorageConnection _connection;

		public JobsManager(
			JobsOptions options,
			IStorage storage,
			IStateChanger stateChanger,
			IProcessingServer server,
			IStorageConnection connection)
		{
			_options = options;
			_storage = storage;
			_stateChanger = stateChanger;
			_server = server;
			_connection = connection;
		}

		public async Task EnqueueAsync(Expression<Action> methodCall)
		{
			if (methodCall == null) throw new ArgumentNullException(nameof(methodCall));

			var method = MethodInvocation.FromExpression(methodCall);
			await EnqueueCore(null, method);
		}

		public async Task EnqueueAsync<T>(Expression<Action<T>> methodCall)
		{
			if (methodCall == null) throw new ArgumentNullException(nameof(methodCall));

			var method = MethodInvocation.FromExpression(methodCall);
			await EnqueueCore(null, method);
		}

		public async Task EnqueueAsync<T>(Expression<Func<T, Task>> methodCall)
		{
			if (methodCall == null) throw new ArgumentNullException(nameof(methodCall));

			var method = MethodInvocation.FromExpression(methodCall);
			await EnqueueCore(null, method);
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

		public Task EnqueueAsync<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset due)
		{
			if (methodCall == null) throw new ArgumentNullException(nameof(methodCall));

			var method = MethodInvocation.FromExpression(methodCall);
			return EnqueueCore(due.UtcDateTime, method);
		}

		public async Task<bool> ChangeStateAsync(int jobId, IState state, string expectedState)
		{
			if (state == null) throw new ArgumentNullException(nameof(state));

			var job = await _connection.GetJobAsync(jobId);
			if (job == null)
			{
				return false;
			}

			if (expectedState != null && !job.StateName.Equals(expectedState, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			await _stateChanger.ChangeStateAsync(job, state, _connection);
			return true;
		}

		private async Task EnqueueCore(DateTime? due, MethodInvocation method)
		{
			var data = InvocationData.Serialize(method);
			var job = new Job(data.Serialize())
			{
				Due = due
			};

			await _connection.StoreJobAsync(job);
			if (job.Due == null)
			{
				_server.Pulse();
			}
		}
	}
}
