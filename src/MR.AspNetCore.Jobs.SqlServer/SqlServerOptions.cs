namespace MR.AspNetCore.Jobs
{
	public class SqlServerOptions
	{
		public string ConnectionString { get; set; }

		public string Schema { get; set; } = "Jobs";

		public string MigrationsHistoryTableName { get; set; } = "__EFMigrationsHistory";
	}
}
