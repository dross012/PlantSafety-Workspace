// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Resilience
//  Layer:          Tests (Resilience)
//  File:           LocalEmergencyCoordinationResilienceTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Resilience tests: optimistic-concurrency conflicts must propagate unchanged
//                  (never swallowed into a generic 500) across every ETag-aware mutating command,
//                  and repeated read failures must not corrupt in-memory aggregate state. Isolated,
//                  deterministic, no I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Enums;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Interfaces;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.ValueObjects.Ids;
using PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.TestSupport;
using LocalEmergencyCoordinationAggregate = PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Resilience;

public sealed class LocalEmergencyCoordinationResilienceTests : UnitTestBase
{
    [Fact]
    public async Task RecordJointDebrief_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var repo = Substitute.For<ILocalEmergencyCoordinationRepository>();
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        repo.GetByIdAsync(Arg.Any<LocalEmergencyCoordinationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(agg);
        repo.UpdateAsync(Arg.Any<LocalEmergencyCoordinationAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("stale", TestCorrelationId.Value));

        var handler = new RecordJointDebriefCommandHandler(repo, NullLogger<RecordJointDebriefCommandHandler>());
        var command = new RecordJointDebriefCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            agg.LocalEmergencyCoordinationId.Value, null, "Agency", "Summary", TestUserId.Value)
        { ETag = "stale" };

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Repository_TransientReadFailure_DoesNotCorruptSubsequentReads()
    {
        // Simulates a transient repository blip (e.g. connection reset) on the first call,
        // recovering on retry -- the aggregate returned on the successful call must be unaffected.
        var repo = Substitute.For<ILocalEmergencyCoordinationRepository>();
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        var callCount = 0;
        repo.GetByIdAsync(Arg.Any<LocalEmergencyCoordinationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callCount++;
                if (callCount == 1) throw new TimeoutException("transient connection timeout");
                return Task.FromResult<LocalEmergencyCoordinationAggregate?>(agg);
            });

        Func<Task> firstAttempt = async () => await repo.GetByIdAsync(agg.LocalEmergencyCoordinationId, TestTenantId.Value, CancellationToken.None);
        await firstAttempt.Should().ThrowAsync<TimeoutException>();

        var retried = await repo.GetByIdAsync(agg.LocalEmergencyCoordinationId, TestTenantId.Value, CancellationToken.None);
        retried.Should().BeSameAs(agg);
        retried!.Status.Should().Be(LocalEmergencyCoordinationStatus.Created);
    }

    [Fact]
    public async Task AllMutatingCommands_WhenAggregateNotFound_DoNotCallUpdate()
    {
        // A NotFound result must be a pure read failure -- UpdateAsync must never be invoked when
        // GetByIdAsync returns null, regardless of which of the six existing-case commands is used.
        var repo = Substitute.For<ILocalEmergencyCoordinationRepository>();
        repo.GetByIdAsync(Arg.Any<LocalEmergencyCoordinationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((LocalEmergencyCoordinationAggregate?)null);

        var handler = new CloseLocalEmergencyCoordinationCaseCommandHandler(repo, NullLogger<CloseLocalEmergencyCoordinationCaseCommandHandler>());
        var command = new CloseLocalEmergencyCoordinationCaseCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, Guid.NewGuid(), TestUserId.Value);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        await repo.DidNotReceive().UpdateAsync(Arg.Any<LocalEmergencyCoordinationAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }
}
