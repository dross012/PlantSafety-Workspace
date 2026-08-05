// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Domain.Aggregates
//  Layer:          Tests (Unit)
//  File:           CognitiveLoadLedgerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the CognitiveLoadLedger aggregate root:
//                   Create, RecordCognitiveLoadReading (including overload-detection event
//                   raising), DefineTaskComplexityProfile, RecalculateWorkloadTrend, Close, and
//                   Archive, plus their guarded lifecycle transitions. Isolated, deterministic,
//                   <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Aggregates;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Events;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Exceptions;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Domain.Aggregates;

public sealed class CognitiveLoadLedgerTests : UnitTestBase
{
    private static CognitiveLoadLedger CreateOpenLedger() =>
        CognitiveLoadLedger.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: Guid.NewGuid(),
            shiftType: ShiftType.Day,
            shiftDateUtc: FixedTimestamp);

    [Fact]
    public void Create_WithValidInputs_OpensLedgerWithNoDomainEvents()
    {
        var ledger = CreateOpenLedger();

        ledger.Status.Should().Be(CognitiveLoadLedgerStatus.Open);
        ledger.TenantId.Should().Be(TestTenantId.Value);
        ledger.CreatedBy.Should().Be(TestUserId);
        ledger.DomainEvents.Should().BeEmpty("Create is a silent substrate, mirroring AdvancedMetricsLedger.Create");
    }

    [Fact]
    public void Create_WithEmptyTenantId_ThrowsArgumentException()
    {
        var act = () => CognitiveLoadLedger.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: Guid.Empty, siteId: null, shiftType: ShiftType.Day, shiftDateUtc: FixedTimestamp);

        act.Should().Throw<ArgumentException>().WithParameterName("tenantId");
    }

    [Fact]
    public void RecordCognitiveLoadReading_WithLowScore_RaisesOnlyReadingRecordedEvent()
    {
        var ledger = CreateOpenLedger();

        var reading = ledger.RecordCognitiveLoadReading(
            workerUserId: TestUserId, taskComplexityProfileId: null, cognitiveLoadScore: 20m,
            measurementMethod: "SelfReport", measuredAtUtc: FixedTimestamp, recordedAtUtc: FixedTimestamp,
            recordedBy: TestUserId, actor: TestActor, correlationId: TestCorrelationId, causationId: TestCausationId);

        reading.WorkloadLevel.Should().Be(CognitiveWorkloadLevel.Low);
        reading.IsOverload.Should().BeFalse();
        ledger.CognitiveLoadReadings.Should().ContainSingle().Which.Should().BeSameAs(reading);
        ledger.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<CognitiveLoadReadingRecordedEvent>();
    }

    [Fact]
    public void RecordCognitiveLoadReading_WithOverloadScore_RaisesReadingRecordedAndOverloadDetectedEvents()
    {
        var ledger = CreateOpenLedger();

        var reading = ledger.RecordCognitiveLoadReading(
            workerUserId: TestUserId, taskComplexityProfileId: null, cognitiveLoadScore: 85m,
            measurementMethod: "NASA-TLX", measuredAtUtc: FixedTimestamp, recordedAtUtc: FixedTimestamp,
            recordedBy: TestUserId, actor: TestActor, correlationId: TestCorrelationId, causationId: TestCausationId);

        reading.WorkloadLevel.Should().Be(CognitiveWorkloadLevel.Overload);
        reading.IsOverload.Should().BeTrue();
        ledger.DomainEvents.Should().HaveCount(2);
        ledger.DomainEvents.OfType<CognitiveLoadReadingRecordedEvent>().Should().ContainSingle();
        ledger.DomainEvents.OfType<CognitiveOverloadDetectedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void RecordCognitiveLoadReading_WhenLedgerClosed_ThrowsInvalidStatusTransitionException()
    {
        var ledger = CreateOpenLedger();
        ledger.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => ledger.RecordCognitiveLoadReading(
            TestUserId, null, 20m, "SelfReport", FixedTimestamp, FixedTimestamp, TestUserId,
            TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CognitiveLoadLedgerInvalidStatusTransitionException>();
    }

    [Fact]
    public void DefineTaskComplexityProfile_WhenOpen_AddsProfileAndRaisesNoDomainEvent()
    {
        var ledger = CreateOpenLedger();

        var profile = ledger.DefineTaskComplexityProfile(
            "CTRL-ROOM-STARTUP", "Control Room Startup", 7, "High-stakes multi-step startup sequence.",
            FixedTimestamp, TestUserId, TestCorrelationId, TestCausationId);

        ledger.TaskComplexityProfiles.Should().ContainSingle().Which.Should().BeSameAs(profile);
        ledger.DomainEvents.Should().BeEmpty("definitional reference data raises no domain event, mirroring Create's silent substrate precedent");
    }

    [Fact]
    public void DefineTaskComplexityProfile_WhenLedgerArchived_ThrowsInvalidStatusTransitionException()
    {
        var ledger = CreateOpenLedger();
        ledger.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        ledger.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => ledger.DefineTaskComplexityProfile(
            "X", "Task X", 5, "desc", FixedTimestamp, TestUserId, TestCorrelationId, TestCausationId);

        act.Should().Throw<CognitiveLoadLedgerInvalidStatusTransitionException>();
    }

    [Fact]
    public void RecalculateWorkloadTrend_WithReadingsInWindow_ComputesMeanAndOverloadCountAndRaisesEvent()
    {
        var ledger = CreateOpenLedger();
        var t0 = FixedTimestamp;
        var t1 = UtcTimestamp.From(FixedUtcNow.AddMinutes(30));

        ledger.RecordCognitiveLoadReading(TestUserId, null, 40m, "SelfReport", t0, t0, TestUserId, TestActor, TestCorrelationId, TestCausationId);
        ledger.RecordCognitiveLoadReading(TestUserId, null, 90m, "SelfReport", t1, t1, TestUserId, TestActor, TestCorrelationId, TestCausationId);

        var record = ledger.RecalculateWorkloadTrend(
            periodStartUtc: t0, periodEndUtc: t1, recalculatedAtUtc: FixedTimestamp, recalculatedBy: TestUserId,
            actor: TestActor, correlationId: TestCorrelationId, causationId: TestCausationId);

        record.ReadingCount.Should().Be(2);
        record.OverloadEventCount.Should().Be(1);
        record.MeanCognitiveLoadScore.Should().Be(65m);
        ledger.WorkloadTrendRecords.Should().ContainSingle();
        ledger.DomainEvents.OfType<WorkloadTrendRecalculatedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void RecalculateWorkloadTrend_WithNoReadingsInWindow_ProducesZeroedRecord()
    {
        var ledger = CreateOpenLedger();
        var farFuture = UtcTimestamp.From(FixedUtcNow.AddDays(1));

        var record = ledger.RecalculateWorkloadTrend(
            FixedTimestamp, farFuture, FixedTimestamp, TestUserId, TestActor, TestCorrelationId, TestCausationId);

        record.ReadingCount.Should().Be(0);
        record.OverloadEventCount.Should().Be(0);
        record.MeanCognitiveLoadScore.Should().Be(0m);
    }

    [Fact]
    public void Close_WhenOpen_TransitionsToClosed()
    {
        var ledger = CreateOpenLedger();

        ledger.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        ledger.Status.Should().Be(CognitiveLoadLedgerStatus.Closed);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ThrowsInvalidStatusTransitionException()
    {
        var ledger = CreateOpenLedger();
        ledger.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => ledger.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CognitiveLoadLedgerInvalidStatusTransitionException>();
    }

    [Fact]
    public void Archive_WhenClosed_TransitionsToArchived()
    {
        var ledger = CreateOpenLedger();
        ledger.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        ledger.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        ledger.Status.Should().Be(CognitiveLoadLedgerStatus.Archived);
    }

    [Fact]
    public void Archive_WhenStillOpen_ThrowsInvalidStatusTransitionException()
    {
        var ledger = CreateOpenLedger();

        var act = () => ledger.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CognitiveLoadLedgerInvalidStatusTransitionException>();
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllPendingEvents()
    {
        var ledger = CreateOpenLedger();
        ledger.RecordCognitiveLoadReading(TestUserId, null, 85m, "SelfReport", FixedTimestamp, FixedTimestamp, TestUserId, TestActor, TestCorrelationId, TestCausationId);
        ledger.DomainEvents.Should().NotBeEmpty();

        ledger.ClearDomainEvents();

        ledger.DomainEvents.Should().BeEmpty();
    }
}
