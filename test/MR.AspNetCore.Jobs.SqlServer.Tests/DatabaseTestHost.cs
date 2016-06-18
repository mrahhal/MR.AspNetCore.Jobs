using System.Data.SqlClient;
using System.Transactions;
using Dapper;

namespace MR.AspNetCore.Jobs
{
	public abstract class DatabaseTestHost : TestHost
	{
		private static bool _sqlObjectInstalled;
		private TransactionScope _transaction;

		public DatabaseTestHost()
		{
			if (!_sqlObjectInstalled)
			{
				CreateAndInitializeDatabaseIfNotExists();
				_sqlObjectInstalled = true;
			}

			_transaction = new TransactionScope(
				TransactionScopeOption.RequiresNew,
				new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
				TransactionScopeAsyncFlowOption.Enabled);
		}

		public override void Dispose()
		{
			base.Dispose();
			_transaction.Dispose();
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
