using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;

namespace MR.AspNetCore.Jobs.Server
{
	public class ExpirationManager : IProcessor, IAdditionalProcessor
	{
		private ILogger _logger;

		private const int MaxBatch = 1000;
		private TimeSpan _delay = TimeSpan.FromSeconds(1);
		private readonly TimeSpan _waitingInterval = TimeSpan.FromHours(1);

		private static readonly string[] Tables =
		{
			"Jobs"
		};

		public ExpirationManager(ILogger<ExpirationManager> logger)
		{
			_logger = logger;
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
					await storage.UseConnectionAsync(async connection =>
					{
						removedCount = await connection.ExecuteAsync($@"
							SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
							DELETE TOP (@count) FROM [Jobs].[{table}] WITH (readpast) WHERE ExpiresAt < @now;",
							new { now = DateTime.UtcNow, count = MaxBatch });
					});

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
