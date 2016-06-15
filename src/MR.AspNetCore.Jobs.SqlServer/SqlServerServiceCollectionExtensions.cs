using Microsoft.Extensions.DependencyInjection;

namespace MR.AspNetCore.Jobs
{
	public static class SqlServerServiceCollectionExtensions
	{
		public static IServiceCollection AddJobsSqlServer(this IServiceCollection services)
		{
			services.AddSingleton<IBootstrapper, SqlServerBootstrapper>();
			services.AddSingleton<IStorage, SqlServerStorage>();
			services.AddSingleton<IStorageConnection, SqlServerStorageConnection>();

			return services;
		}
	}
}
