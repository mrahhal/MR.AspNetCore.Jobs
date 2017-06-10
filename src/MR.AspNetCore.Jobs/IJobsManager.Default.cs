using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
		private ILogger _logger;
		private JobsOptions _options;
		private IStorage _storage;
		private IStateChanger _stateChanger;
		private IProcessingServer _server;
		private IStorageConnection _connection;

		public JobsManager(
			ILogger<JobsManager> logger,
			JobsOptions options,
			IStorage storage,
			IStateChanger stateChanger,
			IProcessingServer server,
			IStorageConnection connection)
		{
			_logger = logger;
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
			var serializedData = data.Serialize();
			var job = new Job(serializedData)
			{
				Due = due
			};

			await _connection.StoreJobAsync(job);

			if (_logger.IsEnabled(LogLevel.Debug))
			{
				var methodCall = string.Format(
					"{0}.{1}({2})",
					method.Type.Name,
					method.Method.Name,
					string.Join(", ", method.Args.Select(arg => Helper.ToJson(arg))));

				if (due == null)
				{
					_logger.LogDebug(
						"Enqueuing a job to be executed immediately: {MethodCall}", methodCall);
				}
				else
				{
					var diff = due.Value - DateTime.UtcNow;
					var after = diff.TotalSeconds < 60 ? $"{Math.Round(diff.TotalSeconds, 2)} secs" :
						(diff.TotalMinutes < 60 ? $"{Math.Round(diff.TotalMinutes, 2)} minutes" :
						(diff.TotalHours < 24 ? $"{Math.Round(diff.TotalHours, 2)} hours" :
						($"{Math.Round(diff.TotalDays, 2)} days")));

					_logger.LogDebug(
						"Enqueuing a job to be executed after {After}: {MethodCall}", after, methodCall);
				}
			}

			if (job.Due == null)
			{
				_server.Pulse();
			}
		}
	}
}
