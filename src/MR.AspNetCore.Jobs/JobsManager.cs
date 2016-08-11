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
		private IProcessingServer _server;
		private IStateChanger _stateChanger;

		public JobsManager(
			JobsOptions options,
			IStorage storage,
			IStateChanger stateChanger,
			IProcessingServer server)
		{
			_options = options;
			_storage = storage;
			_stateChanger = stateChanger;
			_server = server;
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

			using (var connection = _storage.GetConnection())
			{
				var job = await connection.GetJobAsync(jobId);
				if (job == null)
				{
					return false;
				}

				if (expectedState != null && !job.StateName.Equals(expectedState, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}

				await _stateChanger.ChangeStateAsync(job, state, connection);
				return true;
			}
		}

		private async Task EnqueueCore(DateTime? due, MethodInvocation method)
		{
			var data = InvocationData.Serialize(method);
			var job = new Job(data.Serialize());
			job.Due = due;

			using (var connection = _storage.GetConnection())
			{
				await connection.StoreJobAsync(job);
			}
			if (job.Due == null)
			{
				_server.Pulse();
			}
		}
	}
}
