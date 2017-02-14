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
	}
}
