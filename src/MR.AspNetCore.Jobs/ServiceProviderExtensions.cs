using System;
using Microsoft.Extensions.DependencyInjection;

namespace MR.AspNetCore.Jobs
{
	public static class ServiceProviderExtensions
	{
		public static IServiceScope CreateScope(this IServiceProvider provider)
		{
			return provider.GetService<IServiceScopeFactory>().CreateScope();
		}
	}
}
