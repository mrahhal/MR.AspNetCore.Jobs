using System;

namespace MR.AspNetCore.Jobs
{
	/// <summary>
	/// Creates objects.
	/// </summary>
	public interface IJobFactory
	{
		/// <summary>
		/// Creates an object instance.
		/// </summary>
		/// <param name="type">The type of the object to create.</param>
		/// <returns>An object instance.</returns>
		object Create(Type type);
	}

	public static class JobFactoryExtensions
	{
		public static T Create<T>(this IJobFactory @this)
			=> (T)@this.Create(typeof(T));
	}
}
