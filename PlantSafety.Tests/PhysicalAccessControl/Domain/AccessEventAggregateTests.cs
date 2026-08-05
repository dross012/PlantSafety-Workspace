// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.PhysicalAccessControl
//  Layer:          Tests (Unit)
//  File:           AccessEventAggregateTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the AccessEvent aggregate root: ingestion, processing, and
//                  anomaly-flagging lifecycle, invariant-guard tests for illegal transitions,
//                  and domain-event assertions.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates;
using PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Enums;
using PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Events;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.PhysicalAccessControl.Domain;

/// <summary>Unit tests for <see cref="AccessEvent"/> in the PhysicalAccessControl bounded context.</summary>
public sealed class AccessEventAggregateTests : UnitTestBase
{
    private static AccessEvent CreateIngested(
        AccessEventType eventType = AccessEventType.Granted,
        AccessEventDirection direction = AccessEventDirection.In,
        Guid? credentialId = null,
        UtcTimestamp? occurredAtUtc = null) =>
        AccessEvent.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            accessPointId: Guid.NewGuid(),
            credentialId: credentialId ?? Guid.NewGuid(),
            safetyZoneId: Guid.NewGuid(),
            eventType: eventType,
            direction: direction,
            occurredAtUtc: occurredAtUtc);

    // ---- Create ----------------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnAggregate() =>
        CreateIngested().Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusIngested()
    {
        var agg = CreateIngested();
        agg.Status.Should().Be(AccessEventStatus.Ingested);
    }

    [Fact]
    public void Create_ShouldRaiseAccessEventIngestedEvent()
    {
        var agg = CreateIngested();
        agg.DomainEvents.ShouldRaiseSingleEvent<AccessEventIngestedEvent>();
    }

    [Fact]
    public void Create_WithoutExplicitOccurredAtUtc_ShouldDefaultToCreatedAtUtc()
    {
        var agg = CreateIngested(occurredAtUtc: null);
        agg.OccurredAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Create_WithExplicitOccurredAtUtc_ShouldUseGivenTimestamp()
    {
        var occurred = UtcTimestamp.From(FixedUtcNow.AddMinutes(-5));
        var agg = CreateIngested(occurredAtUtc: occurred);
        agg.OccurredAtUtc.Should().Be(occurred);
    }

    [Fact]
    public void Create_WithoutCredential_ShouldAllowNullCredentialId()
    {
        var agg = AccessEvent.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            accessPointId: Guid.NewGuid(),
            credentialId: null,
            eventType: AccessEventType.ForcedEntry);

        agg.CredentialId.Should().BeNull();
    }

    [Fact]
    public void Create_TwoAggregates_ShouldHaveDifferentIds()
    {
        var a1 = CreateIngested();
        var a2 = CreateIngested();
        a1.AccessEventId.Should().NotBe(a2.AccessEventId);
    }

    // ---- MarkProcessed -------------------------------------------------------------

    [Fact]
    public void MarkProcessed_WhenIngested_ShouldTransitionToProcessed()
    {
        var agg = CreateIngested();
        agg.MarkProcessed(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(AccessEventStatus.Processed);
    }

    [Fact]
    public void MarkProcessed_ShouldRaiseEvent()
    {
        var agg = CreateIngested();
        agg.ClearDomainEvents();
        agg.MarkProcessed(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<AccessEventProcessedEvent>();
    }

    [Fact]
    public void MarkProcessed_WhenAlreadyProcessed_ShouldThrowInvalidOperationException()
    {
        var agg = CreateIngested();
        agg.MarkProcessed(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.MarkProcessed(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- FlagAnomaly ---------------------------------------------------------------

    [Fact]
    public void FlagAnomaly_WhenIngested_ShouldTransitionToFlagged()
    {
        var agg = CreateIngested();
        agg.FlagAnomaly("Tailgating", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(AccessEventStatus.Flagged);
    }

    [Fact]
    public void FlagAnomaly_ShouldTrimAndPersistAnomalyType()
    {
        var agg = CreateIngested();
        agg.FlagAnomaly("  Tailgating  ", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.FlaggedAnomalyType.Should().Be("Tailgating");
    }

    [Fact]
    public void FlagAnomaly_FromProcessed_ShouldTransitionToFlagged()
    {
        var agg = CreateIngested();
        agg.MarkProcessed(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.FlagAnomaly("AfterHoursAccess", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(AccessEventStatus.Flagged);
    }

    [Fact]
    public void FlagAnomaly_ShouldRaiseEvent()
    {
        var agg = CreateIngested();
        agg.ClearDomainEvents();
        agg.FlagAnomaly("Tailgating", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<AccessEventFlaggedEvent>();
    }

    [Fact]
    public void FlagAnomaly_WhenAlreadyFlagged_ShouldThrowInvalidOperationException()
    {
        var agg = CreateIngested();
        agg.FlagAnomaly("Tailgating", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.FlagAnomaly("ForcedEntry", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }
}
