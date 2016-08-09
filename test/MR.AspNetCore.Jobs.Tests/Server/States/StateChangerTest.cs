using System;
using FluentAssertions;
using Moq;
using MR.AspNetCore.Jobs.Models;
using Xunit;

namespace MR.AspNetCore.Jobs.Server.States
{
	public class StateChangerTest
	{
		[Fact]
		public void ChangeState()
		{
			// Arrange
			var fixture = Create();
			var job = new Job("data")
			{
				StateName = "bar"
			};
			var state = Mock.Of<IState>(s => s.Name == "s" && s.ExpiresAfter == null);
			var mockTransaction = new Mock<IStorageTransaction>();

			// Act
			fixture.ChangeState(job, state, mockTransaction.Object);

			// Assert
			job.StateName.Should().Be("s");
			job.ExpiresAt.Should().NotHaveValue();
			mockTransaction.Verify(t => t.UpdateJob(job), Times.Once);
			mockTransaction.Verify(t => t.CommitAsync(), Times.Never);
		}

		[Fact]
		public void ChangeState_ExpiresAfter()
		{
			// Arrange
			var fixture = Create();
			var job = new Job("data")
			{
				StateName = "bar"
			};
			var state = Mock.Of<IState>(s => s.Name == "s" && s.ExpiresAfter == TimeSpan.FromHours(1));
			var mockTransaction = new Mock<IStorageTransaction>();

			// Act
			fixture.ChangeState(job, state, mockTransaction.Object);

			// Assert
			job.StateName.Should().Be("s");
			job.ExpiresAt.Should().HaveValue();
			mockTransaction.Verify(t => t.UpdateJob(job), Times.Once);
			mockTransaction.Verify(t => t.CommitAsync(), Times.Never);
		}

		private StateChanger Create() => new StateChanger();
	}
}
