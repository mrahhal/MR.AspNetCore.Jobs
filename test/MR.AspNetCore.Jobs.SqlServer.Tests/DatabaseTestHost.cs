using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs
{
	public abstract class DatabaseTestHost : TestHost
	{
		private static bool _sqlObjectInstalled;

		protected override void PostBuildServices()
		{
			base.PostBuildServices();
			InitializeDatabase();
		}

		public override void Dispose()
		{
			DeleteAllData();
			base.Dispose();
		}

		private void InitializeDatabase()
		{
			if (!_sqlObjectInstalled)
			{
				using (CreateScope())
				{
					var context = GetService<JobsDbContext>();
					context.Database.EnsureDeleted();
					context.Database.Migrate();
					_sqlObjectInstalled = true;
				}
			}
		}

		private void DeleteAllData()
		{
			using (CreateScope())
			{
				var context = GetService<JobsDbContext>();

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
					context.GetDbConnection().Execute(
						"sp_MSforeachtable",
						new { command1 = command },
						commandType: CommandType.StoredProcedure);
				}
			}
		}
	}
}
