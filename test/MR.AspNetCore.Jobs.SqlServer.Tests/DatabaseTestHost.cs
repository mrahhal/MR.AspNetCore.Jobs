using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace MR.AspNetCore.Jobs
{
	public abstract class DatabaseTestHost : TestHost
	{
		private static bool _sqlObjectInstalled;

		public DatabaseTestHost()
		{
			if (!_sqlObjectInstalled)
			{
				CreateAndInitializeDatabaseIfNotExists();
				_sqlObjectInstalled = true;
			}
		}

		public override void Dispose()
		{
			using (var connection = ConnectionUtil.CreateConnection())
			{
				var commands = new[]
				{
					"DISABLE TRIGGER ALL ON ?",
					"ALTER TABLE ? NOCHECK CONSTRAINT ALL",
					"DELETE FROM ?",
					"ALTER TABLE ? CHECK CONSTRAINT ALL",
					"ENABLE TRIGGER ALL ON ?"
				};
				foreach (var command in commands)
				{
					connection.Execute(
						"sp_MSforeachtable",
						new { command1 = command },
						commandType: CommandType.StoredProcedure);
				}
			}
			base.Dispose();
		}

		private static void CreateAndInitializeDatabaseIfNotExists()
		{
			var recreateDatabaseSql = string.Format(@"
IF DB_ID('{0}') IS NOT NULL DROP DATABASE [{0}];
CREATE DATABASE [{0}] COLLATE SQL_Latin1_General_CP1_CS_AS;",
				ConnectionUtil.GetDatabaseName());

			using (var connection = ConnectionUtil.CreateConnection(ConnectionUtil.GetMasterConnectionString()))
			{
				connection.Execute(recreateDatabaseSql);
			}

			using (var connection = ConnectionUtil.CreateConnection())
			{
				SqlServerObjectsInstaller.Install(connection, null);
			}
		}
	}
}
