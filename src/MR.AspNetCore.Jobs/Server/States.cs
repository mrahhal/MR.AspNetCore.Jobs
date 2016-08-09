namespace MR.AspNetCore.Jobs.Server
{
	public static class States
	{
		public const string Schedulued = nameof(Schedulued);
		public const string Enqueued = nameof(Enqueued);
		public const string Processing = nameof(Processing);
		public const string Succeeded = nameof(Succeeded);
		public const string Failed = nameof(Failed);
	}
}
