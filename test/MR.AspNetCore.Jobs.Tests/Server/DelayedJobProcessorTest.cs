using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server.States;
using Xunit;

namespace MR.AspNetCore.Jobs.Server
{
	public class DelayedJobProcessorTest
	{
		private CancellationTokenSource _cancellationTokenSource;
		private ProcessingContext _context;
		private Mock<IStateChanger> _mockStateChanger;
		private Mock<IStorage> _mockStorage;
		private JobsOptions _options;
		private IServiceProvider _provider;
		private Mock<IStorageConnection> _mockStorageConnection;
		private Mock<IStorageTransaction> _mockStorageTransaction;

		public DelayedJobProcessorTest()
		{
			_options = new JobsOptions()
			{
				PollingDelay = 0
			};
			_mockStateChanger = new Mock<IStateChanger>();
			_mockStorage = new Mock<IStorage>();
			_mockStorageConnection = new Mock<IStorageConnection>();
			_mockStorageTransaction = new Mock<IStorageTransaction>();
			_mockStorage.Setup(m => m.GetConnection()).Returns(_mockStorageConnection.Object);
			_mockStorageConnection.Setup(m => m.CreateTransaction()).Returns(_mockStorageTransaction.Object);
			_cancellationTokenSource = new CancellationTokenSource();

			var services = new ServiceCollection();
			services.AddTransient<DelayedJobProcessor>();
			services.AddLogging();
			services.AddSingleton(_options);
			services.AddSingleton(_mockStorage.Object);
			services.AddSingleton(_mockStateChanger.Object);
			services.AddTransient<IJobFactory, JobFactory>();
			services.AddTransient<NoRetryJob>();
			_provider = services.BuildServiceProvider();

			_context = new ProcessingContext(_provider, _mockStorage.Object, null, _cancellationTokenSource.Token);
		}

		[Fact]
		public async Task ProcessAsync_CancellationTokenCancelled_ThrowsImmediately()
		{
			// Arrange
			_cancellationTokenSource.Cancel();
			var fixture = Create();

			// Act
			await Assert.ThrowsAsync<OperationCanceledException>(() => fixture.ProcessAsync(_context));
		}

		[Fact]
		public async Task ProcessAsync()
		{
			// Arrange
			var job = new Job(
				InvocationData.Serialize(
					MethodInvocation.FromExpression(() => Method())).Serialize());

			var mockFetchedJob = Mock.Get(Mock.Of<IFetchedJob>(fj => fj.JobId == 42));

			_mockStorageConnection
				.Setup(m => m.FetchNextJobAsync())
				.ReturnsAsync(mockFetchedJob.Object).Verifiable();

			_mockStorageConnection
				.Setup(m => m.GetJobAsync(42))
				.ReturnsAsync(job).Verifiable();

			var fixture = Create();

			// Act
			await fixture.ProcessAsync(_context);

			// Assert
			_mockStorageConnection.VerifyAll();
			_mockStateChanger.Verify(m => m.ChangeState(job, It.IsAny<SucceededState>(), It.IsAny<IStorageTransaction>()));
			mockFetchedJob.Verify(m => m.Requeue(), Times.Never);
			mockFetchedJob.Verify(m => m.RemoveFromQueue());
		}

		[Fact]
		public async Task ProcessAsync_Exception()
		{
			// Arrange
			var job = new Job(
				InvocationData.Serialize(
					MethodInvocation.FromExpression(() => Throw())).Serialize());

			var mockFetchedJob = Mock.Get(Mock.Of<IFetchedJob>(fj => fj.JobId == 42));

			_mockStorageConnection
				.Setup(m => m.FetchNextJobAsync())
				.ReturnsAsync(mockFetchedJob.Object);

			_mockStorageConnection
				.Setup(m => m.GetJobAsync(42))
				.ReturnsAsync(job);

			_mockStateChanger.Setup(m => m.ChangeState(job, It.IsAny<IState>(), It.IsAny<IStorageTransaction>()))
				.Throws<Exception>();

			var fixture = Create();

			// Act
			await fixture.ProcessAsync(_context);

			// Assert
			job.Retries.Should().Be(0);
			mockFetchedJob.Verify(m => m.Requeue());
		}

		[Fact]
		public async Task ProcessAsync_JobThrows()
		{
			// Arrange
			var job = new Job(
				InvocationData.Serialize(
					MethodInvocation.FromExpression(() => Throw())).Serialize());

			var mockFetchedJob = Mock.Get(Mock.Of<IFetchedJob>(fj => fj.JobId == 42));

			_mockStorageConnection
				.Setup(m => m.FetchNextJobAsync())
				.ReturnsAsync(mockFetchedJob.Object).Verifiable();

			_mockStorageConnection
				.Setup(m => m.GetJobAsync(42))
				.ReturnsAsync(job).Verifiable();

			var fixture = Create();

			// Act
			await fixture.ProcessAsync(_context);

			// Assert
			job.Retries.Should().Be(1);
			_mockStorageTransaction.Verify(m => m.UpdateJob(job));
			_mockStorageConnection.VerifyAll();
			_mockStateChanger.Verify(m => m.ChangeState(job, It.IsAny<ScheduledState>(), It.IsAny<IStorageTransaction>()));
			mockFetchedJob.Verify(m => m.RemoveFromQueue());
		}

		[Fact]
		public async Task ProcessAsync_JobThrows_WithNoRetry()
		{
			// Arrange
			var job = new Job(
				InvocationData.Serialize(
					MethodInvocation.FromExpression<NoRetryJob>(j => j.Throw())).Serialize());

			var mockFetchedJob = Mock.Get(Mock.Of<IFetchedJob>(fj => fj.JobId == 42));

			_mockStorageConnection
				.Setup(m => m.FetchNextJobAsync())
				.ReturnsAsync(mockFetchedJob.Object);

			_mockStorageConnection
				.Setup(m => m.GetJobAsync(42))
				.ReturnsAsync(job);

			var fixture = Create();

			// Act
			await fixture.ProcessAsync(_context);

			// Assert
			_mockStateChanger.Verify(m => m.ChangeState(job, It.IsAny<FailedState>(), It.IsAny<IStorageTransaction>()));
		}

		private DelayedJobProcessor Create()
			=> _provider.GetService<DelayedJobProcessor>();

		public static void Method() { }

		public static void Throw() { throw new Exception(); }

		private class NoRetryJob : IRetryable
		{
			public RetryBehavior RetryBehavior => new RetryBehavior(false);
			public void Throw() { throw new Exception(); }
		}
	}
}
