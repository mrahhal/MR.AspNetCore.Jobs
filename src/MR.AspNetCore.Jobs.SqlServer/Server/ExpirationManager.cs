using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs.Server
{
	public class ExpirationManager : IProcessor, IAdditionalProcessor
	{
		private ILogger _logger;
		private SqlServerOptions _options;
		private IServiceProvider _provider;

		private const int MaxBatch = 1000;
		private TimeSpan _delay = TimeSpan.FromSeconds(1);
		private readonly TimeSpan _waitingInterval = TimeSpan.FromHours(1);

		private static readonly string[] Tables =
		{
			nameof(JobsDbContext.Jobs)
		};

		public ExpirationManager(
			ILogger<ExpirationManager> logger,
			SqlServerOptions options,
			IServiceProvider provider)
		{
			_logger = logger;
			_options = options;
			_provider = provider;
		}

		public async Task ProcessAsync(ProcessingContext context)
		{
			_logger.CollectingExpiredEntities();

			var storage = context.Storage as SqlServerStorage;

			foreach (var table in Tables)
			{
				var removedCount = 0;
				do
				{
					using (var scope = _provider.CreateScope())
					{
						var provider = scope.ServiceProvider;
						var jobsDbContext = provider.GetService<JobsDbContext>();
						var connection = jobsDbContext.GetDbConnection();

						removedCount = await connection.ExecuteAsync($@"
							SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
							DELETE TOP (@count) FROM [{_options.Schema}].[{table}] WITH (readpast) WHERE ExpiresAt < @now;",
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
	}
}
