namespace MR.AspNetCore.Jobs
{
	public static class JobFactoryExtensions
	{
		public static T Create<T>(this IJobFactory @this)
		{
			return (T)@this.Create(typeof(T));
		}
	}
}
