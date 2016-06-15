using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerStorage : IStorage
	{
		private string _connectionString;
		private ILoggerFactory _loggerFactory;

		public SqlServerStorage(
			IOptions<SqlServerOptions> options,
			ILoggerFactory loggerFactory)
		{
			_connectionString = options.Value.ConnectionString;
			_loggerFactory = loggerFactory;
		}

		public void Initialize()
		{
			UseConnection(connection =>
			{
				SqlServerObjectsInstaller.Install(
					connection,
					_loggerFactory.CreateLogger(typeof(SqlServerObjectsInstaller)));
			});
		}

		public IStorageConnection GetConnection() => new SqlServerStorageConnection(this);

		internal void UseConnection(Action<SqlConnection> action)
		{
			UseConnection(connection =>
			{
				action(connection);
				return true;
			});
		}

		internal T UseConnection<T>(Func<SqlConnection, T> func)
		{
			SqlConnection connection = null;

			try
			{
				connection = CreateAndOpenConnection();
				return func(connection);
			}
			finally
			{
				ReleaseConnection(connection);
			}
		}

		internal Task UseConnectionAsync(Func<SqlConnection, Task> action)
		{
			return UseConnectionAsync(async connection =>
			{
				await action(connection);
				return true;
			});
		}

		internal async Task<T> UseConnectionAsync<T>(Func<SqlConnection, Task<T>> func)
		{
			SqlConnection connection = null;

			try
			{
				connection = CreateAndOpenConnection();
				return await func(connection);
			}
			finally
			{
				ReleaseConnection(connection);
			}
		}

		internal void UseTransaction(Action<SqlConnection, SqlTransaction> action)
		{
			UseTransaction((connection, transaction) =>
			{
				action(connection, transaction);
				return true;
			}, null);
		}

		internal T UseTransaction<T>(Func<SqlConnection, SqlTransaction, T> func, IsolationLevel? isolationLevel)
		{
			return UseConnection(connection =>
			{
				T result;
				using (var transaction = CreateTransaction(connection, isolationLevel))
				{
					result = func(connection, transaction);
					transaction.Commit();
				}
				return result;
			});
		}

		internal SqlConnection CreateAndOpenConnection()
		{
			var connection = new SqlConnection(_connectionString);
			connection.Open();
			return connection;
		}

		internal void ReleaseConnection(IDbConnection connection)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			connection.Dispose();
		}

		private SqlTransaction CreateTransaction(SqlConnection connection, IsolationLevel? isolationLevel)
		{
			return
				isolationLevel == null ?
				connection.BeginTransaction() :
				connection.BeginTransaction(isolationLevel.Value);
		}
	}
}
