using System;
using System.Threading;
using System.Threading.Tasks;
using MR.AspNetCore.Jobs.Server;

namespace MR.AspNetCore.Jobs.Util
{
	public static class WaitHandleEx
	{
		public static Task WaitAnyAsync(WaitHandle handle1, WaitHandle handle2, TimeSpan timeout)
		{
			var t1 = handle1.WaitOneAsync(timeout);
			var t2 = handle2.WaitOneAsync(timeout);
			return Task.WhenAny(t1, t2);
		}
	}
}
