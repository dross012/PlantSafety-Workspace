// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Pipeline
//  Layer:          Tests (Unit)
//  File:           WorkflowInstanceLockBehaviorTests.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for WorkflowInstanceLockBehavior -- proves it acquires/releases a
//                  distributed lock keyed by WorkflowInstanceId for IWorkflowInstanceScopedCommand
//                  requests, and passes every other request straight through with no locking.
//                  Real acquire/release/timeout/serialization behavior of the underlying
//                  SQL-Server-backed provider is covered by
//                  PlantSafety.IntegrationTests.Infrastructure.Concurrency.SqlServerDistributedLockProviderTests.
// ============================================================================
using MediatR;
using NSubstitute;
using PlantSafety.Application.Workflow.Behaviors;
using PlantSafety.Application.Workflow.Commands;
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.Tests.Application.Pipeline;

public sealed class WorkflowInstanceLockBehaviorTests : UnitTestBase
{
    private sealed record ScopedTestCommand(Guid WorkflowInstanceId) : IRequest<Unit>, IWorkflowInstanceScopedCommand;

    private sealed record UnscopedTestCommand : IRequest<Unit>;

    private sealed class RecordingAsyncDisposable : IAsyncDisposable
    {
        public bool Disposed { get; private set; }

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task Handle_ForScopedCommand_AcquiresLockKeyedByWorkflowInstanceId_AndReleasesAfterNext()
    {
        var workflowInstanceId = Guid.NewGuid();
        var lockProvider = Substitute.For<IDistributedLockProvider>();
        var recordingHandle = new RecordingAsyncDisposable();
        lockProvider.AcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(recordingHandle);

        var behavior = new WorkflowInstanceLockBehavior<ScopedTestCommand, Unit>(lockProvider, NullLogger<WorkflowInstanceLockBehavior<ScopedTestCommand, Unit>>());
        var next = Substitute.For<RequestHandlerDelegate<Unit>>();
        next.Invoke().Returns(Unit.Value);

        await behavior.Handle(new ScopedTestCommand(workflowInstanceId), next, CancellationToken.None);

        await lockProvider.Received(1).AcquireAsync(
            $"Workflow:Instance:{workflowInstanceId:D}",
            Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>());
        await next.Received(1).Invoke();
        recordingHandle.Disposed.Should().BeTrue("the lock handle must be released once the handler pipeline completes");
    }

    [Fact]
    public async Task Handle_ForScopedCommand_ReleasesLockEvenWhenNextThrows()
    {
        var workflowInstanceId = Guid.NewGuid();
        var lockProvider = Substitute.For<IDistributedLockProvider>();
        var recordingHandle = new RecordingAsyncDisposable();
        lockProvider.AcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(recordingHandle);

        var behavior = new WorkflowInstanceLockBehavior<ScopedTestCommand, Unit>(lockProvider, NullLogger<WorkflowInstanceLockBehavior<ScopedTestCommand, Unit>>());
        var next = Substitute.For<RequestHandlerDelegate<Unit>>();
        next.Invoke().Returns<Unit>(_ => throw new InvalidOperationException("handler failed"));

        var act = async () => await behavior.Handle(new ScopedTestCommand(workflowInstanceId), next, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        recordingHandle.Disposed.Should().BeTrue("the lock must be released even when the wrapped handler throws");
    }

    [Fact]
    public async Task Handle_ForNonScopedCommand_NeverAcquiresALock()
    {
        var lockProvider = Substitute.For<IDistributedLockProvider>();
        var behavior = new WorkflowInstanceLockBehavior<UnscopedTestCommand, Unit>(lockProvider, NullLogger<WorkflowInstanceLockBehavior<UnscopedTestCommand, Unit>>());
        var next = Substitute.For<RequestHandlerDelegate<Unit>>();
        next.Invoke().Returns(Unit.Value);

        await behavior.Handle(new UnscopedTestCommand(), next, CancellationToken.None);

        await lockProvider.DidNotReceiveWithAnyArgs().AcquireAsync(default!, default, default);
        await next.Received(1).Invoke();
    }
}
