using System.Data.SqlClient;
using System.Threading;
using System.Transactions;
using Dapper;

namespace MR.AspNetCore.Jobs
{
	public abstract class DatabaseTestHost : TestHost
	{
		private static readonly object GlobalLock = new object();
		private static bool _sqlObjectInstalled;
		private TransactionScope _transaction;

		public DatabaseTestHost()
		{
			Monitor.Enter(GlobalLock);

			if (!_sqlObjectInstalled)
			{
				CreateAndInitializeDatabaseIfNotExists();
				_sqlObjectInstalled = true;
			}

			_transaction = new TransactionScope(
				TransactionScopeOption.RequiresNew,
				new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted });
		}

		public override void Dispose()
		{
			base.Dispose();

			try
			{
				_transaction.Dispose();
			}
			finally
			{
				Monitor.Exit(GlobalLock);
			}
		}

		private static void CreateAndInitializeDatabaseIfNotExists()
		{
			var recreateDatabaseSql = string.Format(
				@"if db_id('{0}') is null create database [{0}] COLLATE SQL_Latin1_General_CP1_CS_AS",
				ConnectionUtil.GetDatabaseName());

			using (var connection = new SqlConnection(ConnectionUtil.GetMasterConnectionString()))
			{
				connection.Execute(recreateDatabaseSql);
			}

			using (var connection = new SqlConnection(ConnectionUtil.GetConnectionString()))
			{
				SqlServerObjectsInstaller.Install(connection, null);
			}
		}
	}
}
