using System;

namespace MR.AspNetCore.Jobs
{
	public class RetryBehavior
	{
		public static readonly int DefaultRetryCount;
		public static readonly Func<int, int> DefaultRetryInThunk;

		public static readonly RetryBehavior DefaultRetry;
		public static readonly RetryBehavior NoRetry;

		private static Random _random = new Random();

		private Func<int, int> _retryInThunk;

		static RetryBehavior()
		{
			DefaultRetryCount = 25;
			DefaultRetryInThunk = retries =>
				(int)Math.Round(Math.Pow(retries - 1, 4) + 15 + (_random.Next(30) * (retries)));

			DefaultRetry = new RetryBehavior(true);
			NoRetry = new RetryBehavior(false);
		}

		public RetryBehavior(bool retry)
			: this(retry, DefaultRetryCount, DefaultRetryInThunk)
		{
		}

		public RetryBehavior(bool retry, int retryCount, Func<int, int> retryInThunk)
		{
			if (retry)
			{
				if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), "Can't be negative.");
			}

			Retry = retry;
			RetryCount = retryCount;
			_retryInThunk = retryInThunk ?? DefaultRetryInThunk;
		}

		public Random Random => _random;

		public bool Retry { get; }

		public int RetryCount { get; }

		public int RetryIn(int retries)
		{
			return _retryInThunk(retries);
		}
	}
}
