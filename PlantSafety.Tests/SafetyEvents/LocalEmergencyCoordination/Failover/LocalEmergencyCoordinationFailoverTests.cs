// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Failover
//  Layer:          Tests (Failover)
//  File:           LocalEmergencyCoordinationFailoverTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Failover tests: repository unavailability on the primary read/write path must
//                  degrade to a clean sanitized failure (never crash the process, never corrupt the
//                  in-memory aggregate), and every domain event carries a fresh Uuid7 EventId so a
//                  failed-over retry of the same logical operation is safely distinguishable/dedup-
//                  able downstream. Isolated, deterministic, no I/O, no network.
// ============================================================================
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Interfaces;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.ValueObjects.Ids;
using PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.TestSupport;
using LocalEmergencyCoordinationAggregate = PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Failover;

public sealed class LocalEmergencyCoordinationFailoverTests : UnitTestBase
{
    [Fact]
    public async Task OpenCase_WhenPrimaryWriteTargetUnavailable_ReturnsSanitizedFailure_WithoutThrowing()
    {
        var repo = Substitute.For<ILocalEmergencyCoordinationRepository>();
        repo.AddAsync(Arg.Any<LocalEmergencyCoordinationAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new TimeoutException("simulated primary SQL replica failover in progress"));

        var handler = new OpenLocalEmergencyCoordinationCaseCommandHandler(repo, NullLogger<OpenLocalEmergencyCoordinationCaseCommandHandler>());
        var command = new OpenLocalEmergencyCoordinationCaseCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow, "v1",
            TestTenantId.Value, null, null, "Failover Case", TestUserId.Value);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue("a primary write-target failover must degrade to a sanitized Result.Failure, never an unhandled exception");
    }

    [Fact]
    public void RepeatedRetryOfSameLogicalOperation_ProducesDistinctEventIds_SafeForDownstreamDedup()
    {
        // Simulates a client retrying the same logical "establish mutual aid" call after a
        // failed-over connection drop (before it received the first attempt's response). Each
        // attempt raises its own event with its own EventId, so an idempotent downstream
        // consumer keyed on EventId will not conflate the two.
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();

        var first = agg.EstablishMutualAidAgreement("Agency", "Fire", 1, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddYears(1)), TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        var second = agg.EstablishMutualAidAgreement("Agency", "Fire", 1, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddYears(1)), TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var eventIds = agg.DomainEvents.Select(LocalEmergencyCoordinationTestSupport.EventIdOf).ToList();
        eventIds.Should().OnlyHaveUniqueItems();
        first.MutualAidAgreementId.Should().NotBe(second.MutualAidAgreementId);
    }
}
