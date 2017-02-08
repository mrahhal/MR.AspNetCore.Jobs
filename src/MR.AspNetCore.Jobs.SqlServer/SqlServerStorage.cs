using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerStorage : IStorage
	{
		private IServiceProvider _provider;
		private ILogger _logger;

		public SqlServerStorage(
			IServiceProvider provider,
			ILogger<SqlServerStorage> logger)
		{
			_provider = provider;
			_logger = logger;
		}

		public async Task InitializeAsync(CancellationToken cancellationToken)
		{
			using (var scope = _provider.CreateScope())
			{
				if (cancellationToken.IsCancellationRequested) return;

				var provider = scope.ServiceProvider;
				var context = provider.GetRequiredService<JobsDbContext>();

				_logger.LogDebug("Ensuring all migrations are applied to Jobs database.");
				await context.Database.MigrateAsync(cancellationToken);
			}
		}

		//public IStorageConnection GetConnection() => _provider.GetRequiredService<IStorageConnection>();

		//internal void UseConnection(Action<SqlConnection> action)
		//{
		//	UseConnection(connection =>
		//	{
		//		action(connection);
		//		return true;
		//	});
		//}

		//internal T UseConnection<T>(Func<SqlConnection, T> func)
		//{
		//	SqlConnection connection = null;

		//	try
		//	{
		//		connection = CreateAndOpenConnection();
		//		return func(connection);
		//	}
		//	finally
		//	{
		//		ReleaseConnection(connection);
		//	}
		//}

		//internal Task UseConnectionAsync(Func<SqlConnection, Task> action)
		//{
		//	return UseConnectionAsync(async connection =>
		//	{
		//		await action(connection);
		//		return true;
		//	});
		//}

		//internal async Task<T> UseConnectionAsync<T>(Func<SqlConnection, Task<T>> func)
		//{
		//	SqlConnection connection = null;

		//	try
		//	{
		//		connection = CreateAndOpenConnection();
		//		return await func(connection);
		//	}
		//	finally
		//	{
		//		ReleaseConnection(connection);
		//	}
		//}

		//internal void UseTransaction(Action<SqlConnection, SqlTransaction> action, IsolationLevel? isolationLevel = null)
		//{
		//	UseTransaction((connection, transaction) =>
		//	{
		//		action(connection, transaction);
		//		return true;
		//	}, isolationLevel);
		//}

		//internal Task UseTransactionAsync(Func<SqlConnection, SqlTransaction, Task> func, IsolationLevel? isolationLevel = null)
		//{
		//	return UseTransactionAsync(async (connection, transaction) =>
		//	{
		//		await func(connection, transaction);
		//		return true;
		//	}, isolationLevel);
		//}

		//internal T UseTransaction<T>(Func<SqlConnection, SqlTransaction, T> func, IsolationLevel? isolationLevel = null)
		//{
		//	return UseConnection(connection =>
		//	{
		//		T result;
		//		using (var transaction = CreateTransaction(connection, isolationLevel))
		//		{
		//			result = func(connection, transaction);
		//			transaction.Commit();
		//		}
		//		return result;
		//	});
		//}

		//internal async Task<T> UseTransactionAsync<T>(Func<SqlConnection, SqlTransaction, Task<T>> func, IsolationLevel? isolationLevel = null)
		//{
		//	return await UseConnectionAsync(async connection =>
		//	{
		//		T result;
		//		using (var transaction = CreateTransaction(connection, isolationLevel))
		//		{
		//			result = await func(connection, transaction);
		//			transaction.Commit();
		//		}
		//		return result;
		//	});
		//}

		//internal SqlConnection CreateAndOpenConnection()
		//{
		//	var connection = new SqlConnection(_connectionString);
		//	connection.Open();
		//	return connection;
		//}

		//internal void ReleaseConnection(IDbConnection connection)
		//{
		//	if (connection == null) throw new ArgumentNullException(nameof(connection));

		//	connection.Dispose();
		//}

		//private SqlTransaction CreateTransaction(SqlConnection connection, IsolationLevel? isolationLevel)
		//{
		//	return
		//		isolationLevel == null ?
		//		connection.BeginTransaction() :
		//		connection.BeginTransaction(isolationLevel.Value);
		//}
	}
}
