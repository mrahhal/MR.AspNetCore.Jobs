using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs.Server
{
	public abstract class EFCoreExpirationManager<TContext> : IProcessor, IAdditionalProcessor
		where TContext : EFCoreJobsDbContext
	{
		protected ILogger _logger;
		protected EFCoreOptions _options;
		protected IServiceProvider _provider;

		private const int MaxBatch = 1000;
		private TimeSpan _delay = TimeSpan.FromSeconds(1);
		private readonly TimeSpan _waitingInterval = TimeSpan.FromHours(1);

		private static readonly string[] Tables =
		{
			nameof(EFCoreJobsDbContext.Jobs)
		};

		public EFCoreExpirationManager(
			ILogger logger,
			EFCoreOptions options,
			IServiceProvider provider)
		{
			_logger = logger;
			_options = options;
			_provider = provider;
		}

		public async Task ProcessAsync(ProcessingContext context)
		{
			_logger.CollectingExpiredEntities();

			foreach (var table in Tables)
			{
				var removedCount = 0;
				do
				{
					using (var scope = _provider.CreateScope())
					{
						var provider = scope.ServiceProvider;
						var jobsDbContext = provider.GetService<TContext>();
						var connection = jobsDbContext.GetDbConnection();

						removedCount = await connection.ExecuteAsync(
							CreateDeleteTopQuery(_options.Schema, table),
							new { now = DateTime.UtcNow, count = MaxBatch });
					}

					if (removedCount != 0)
					{
						await context.WaitAsync(_delay);
						context.ThrowIfStopping();
					}
				} while (removedCount != 0);
			}

			await context.WaitAsync(_waitingInterval);
		}

		/// <summary>
		/// Creates a DELETE TOP sql query that uses @count and @now params.
		/// </summary>
		protected abstract string CreateDeleteTopQuery(string schema, string table);
	}
}
