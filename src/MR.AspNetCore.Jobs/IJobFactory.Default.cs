using System;
using Microsoft.Extensions.DependencyInjection;

namespace MR.AspNetCore.Jobs
{
	/// <summary>
	/// An <see cref="IJobFactory"/> that uses an <see cref="IServiceProvider"/> to create jobs.
	/// </summary>
	public class JobFactory : IJobFactory
	{
		private IServiceProvider _provider;

		public JobFactory(IServiceProvider provider)
		{
			_provider = provider;
		}

		public object Create(Type type)
		{
			return _provider.GetRequiredService(type);
		}
	}
}
