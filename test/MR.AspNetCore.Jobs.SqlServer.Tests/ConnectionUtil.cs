using System;
using System.Data.SqlClient;

namespace MR.AspNetCore.Jobs
{
	public static class ConnectionUtil
	{
		private const string DatabaseVariable = "Jobs_SqlServer_DatabaseName";
		private const string ConnectionStringTemplateVariable = "Jobs_SqlServer_ConnectionStringTemplate";

		private const string MasterDatabaseName = "master";
		private const string DefaultDatabaseName = @"MR.AspNetCore.Jobs.SqlServer.Tests";

		private const string DefaultConnectionStringTemplate = @"Server=.\sqlexpress;Database={0};Trusted_Connection=True;";

		public static string GetDatabaseName()
		{
			return Environment.GetEnvironmentVariable(DatabaseVariable) ?? DefaultDatabaseName;
		}

		public static string GetMasterConnectionString()
		{
			return string.Format(GetConnectionStringTemplate(), MasterDatabaseName);
		}

		public static string GetConnectionString()
		{
			return string.Format(GetConnectionStringTemplate(), GetDatabaseName());
		}

		private static string GetConnectionStringTemplate()
		{
			return
				Environment.GetEnvironmentVariable(ConnectionStringTemplateVariable)
				?? DefaultConnectionStringTemplate;
		}

		public static SqlConnection CreateConnection()
		{
			var connection = new SqlConnection(GetConnectionString());
			connection.Open();
			return connection;
		}
	}
}
