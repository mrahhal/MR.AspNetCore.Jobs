using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs
{
	public abstract class EFCoreStorage<TContext> : IStorage
		where TContext : EFCoreJobsDbContext
	{
		protected IServiceProvider _provider;
		protected ILogger _logger;

		public EFCoreStorage(
			IServiceProvider provider,
			ILogger logger)
		{
			_provider = provider;
			_logger = logger;
		}

		public virtual async Task InitializeAsync(CancellationToken cancellationToken)
		{
			using (var scope = _provider.CreateScope())
			{
				if (cancellationToken.IsCancellationRequested) return;

				var provider = scope.ServiceProvider;
				var context = provider.GetRequiredService<TContext>();

				_logger.LogDebug("Ensuring all migrations are applied to Jobs storage.");
				await context.Database.MigrateAsync(cancellationToken);
			}
		}
	}
}
