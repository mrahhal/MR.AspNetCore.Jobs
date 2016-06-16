using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace MR.AspNetCore.Jobs
{
	public class InvocationDataTest
	{
		[Fact]
		public void Serialization()
		{
			var m = InvocationData.Serialize(
				MethodInvocation.FromExpression(() => Foo(1, 2.2, "f", new DateTime(42))))
				.Deserialize();
			m.Args.ElementAt(0).Should().Be(1);
			m.Args.ElementAt(1).Should().Be(2.2);
			m.Args.ElementAt(2).Should().Be("f");
			m.Args.ElementAt(3).Should().Be(new DateTime(42));
		}

		public static void Foo(int a, double c, string d, DateTime e)
		{
		}
	}
}
