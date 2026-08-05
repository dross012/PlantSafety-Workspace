// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Sustainability.AdvancedMetrics.Domain
//  Layer:          Tests (Unit)
//  File:           AdvancedMetricsLedgerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the AdvancedMetricsLedger aggregate root: Create
//                  guards, all five Record* behaviors, SetSustainabilityTarget/UpdateTargetProgress
//                  (including the at-risk domain-event hook), and the Open -> Closed -> Archived
//                  lifecycle transitions (including rejection of out-of-order transitions and
//                  metric recording once no longer Open). Isolated, deterministic, <50ms per test.
//                  No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Aggregates;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Enums;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Events;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Exceptions;

namespace PlantSafety.Tests.Sustainability.AdvancedMetrics.Domain;

public sealed class AdvancedMetricsLedgerTests : UnitTestBase
{
    private static readonly UtcTimestamp PeriodStart = UtcTimestamp.From(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly UtcTimestamp PeriodEnd = UtcTimestamp.From(new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc));

    private AdvancedMetricsLedger OpenLedger() =>
        AdvancedMetricsLedger.Create(
            createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId, causationId: TestCausationId,
            actor: TestActor, tenantId: TestTenantId.Value, siteId: null,
            reportingPeriodStartUtc: PeriodStart, reportingPeriodEndUtc: PeriodEnd);

    // ---- Create -------------------------------------------------------------

    [Fact]
    public void Create_WithValidArguments_InitializesOpenLedgerWithNoChildren()
    {
        var ledger = OpenLedger();

        ledger.Status.Should().Be(AdvancedMetricsLedgerStatus.Open);
        ledger.TenantId.Should().Be(TestTenantId.Value);
        ledger.CarbonEmissionRecords.Should().BeEmpty();
        ledger.EnergyIntensityMetrics.Should().BeEmpty();
        ledger.WaterIntensityMetrics.Should().BeEmpty();
        ledger.WasteDiversionRecords.Should().BeEmpty();
        ledger.SustainabilityTargets.Should().BeEmpty();
        ledger.DomainEvents.Should().BeEmpty("opening a ledger is silent -- only recorded entries raise events");
    }

    [Fact]
    public void Create_WithEmptyTenantId_Throws()
    {
        var act = () => AdvancedMetricsLedger.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: Guid.Empty, siteId: null, reportingPeriodStartUtc: PeriodStart, reportingPeriodEndUtc: PeriodEnd);

        act.Should().Throw<ArgumentException>().WithParameterName("tenantId");
    }

    [Fact]
    public void Create_WithEndBeforeStart_Throws()
    {
        var act = () => AdvancedMetricsLedger.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, siteId: null, reportingPeriodStartUtc: PeriodEnd, reportingPeriodEndUtc: PeriodStart);

        act.Should().Throw<ArgumentException>().WithParameterName("reportingPeriodEndUtc");
    }

    // ---- RecordCarbonEmission -------------------------------------------------

    [Fact]
    public void RecordCarbonEmission_OnOpenLedger_AddsRecordAndRaisesEvent()
    {
        var ledger = OpenLedger();

        var record = ledger.RecordCarbonEmission(
            GhgScope.Scope1, "Boiler House 2", 12.5m, "GHG-Protocol-2026.1",
            FixedTimestamp, FixedTimestamp, TestUserId, TestActor, TestCorrelationId, TestCausationId);

        ledger.CarbonEmissionRecords.Should().ContainSingle().Which.Should().BeSameAs(record);
        ledger.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<CarbonEmissionRecordedEvent>();
    }

    [Fact]
    public void RecordCarbonEmission_OnClosedLedger_ThrowsInvalidTransition()
    {
        var ledger = OpenLedger();
        ledger.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => ledger.RecordCarbonEmission(
            GhgScope.Scope1, "x", 1m, "v1", FixedTimestamp, FixedTimestamp, TestUserId, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<AdvancedMetricsInvalidStatusTransitionException>();
    }

    // ---- RecordEnergyIntensityMetric -------------------------------------------------

    [Fact]
    public void RecordEnergyIntensityMetric_OnOpenLedger_AddsMetricAndRaisesEvent()
    {
        var ledger = OpenLedger();

        var metric = ledger.RecordEnergyIntensityMetric(
            1000m, 500m, "Method-1", FixedTimestamp, FixedTimestamp, TestUserId, TestActor, TestCorrelationId, TestCausationId);

        ledger.EnergyIntensityMetrics.Should().ContainSingle().Which.Should().BeSameAs(metric);
        ledger.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<EnergyIntensityMetricRecordedEvent>();
    }

    // ---- RecordWaterIntensityMetric -------------------------------------------------

    [Fact]
    public void RecordWaterIntensityMetric_OnOpenLedger_AddsMetricAndRaisesEvent()
    {
        var ledger = OpenLedger();

        var metric = ledger.RecordWaterIntensityMetric(
            300m, 500m, "Method-1", FixedTimestamp, FixedTimestamp, TestUserId, TestActor, TestCorrelationId, TestCausationId);

        ledger.WaterIntensityMetrics.Should().ContainSingle().Which.Should().BeSameAs(metric);
        ledger.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<WaterIntensityMetricRecordedEvent>();
    }

    // ---- RecordWasteDiversion -------------------------------------------------

    [Fact]
    public void RecordWasteDiversion_OnOpenLedger_AddsRecordAndRaisesEvent()
    {
        var ledger = OpenLedger();

        var record = ledger.RecordWasteDiversion(
            1000m, 750m, "Method-1", FixedTimestamp, FixedTimestamp, TestUserId, TestActor, TestCorrelationId, TestCausationId);

        ledger.WasteDiversionRecords.Should().ContainSingle().Which.Should().BeSameAs(record);
        ledger.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<WasteDiversionRecordedEvent>();
    }

    // ---- SetSustainabilityTarget / UpdateTargetProgress -------------------------------------------------

    [Fact]
    public void SetSustainabilityTarget_OnOpenLedger_AddsTargetAndRaisesEvent()
    {
        var ledger = OpenLedger();

        var target = ledger.SetSustainabilityTarget(
            SustainabilityTargetType.WasteDiversionRate, baselineValue: 40m, targetValue: 75m,
            targetDateUtc: UtcTimestamp.From(FixedUtcNow.AddYears(1)), setAtUtc: FixedTimestamp,
            setBy: TestUserId, actor: TestActor, correlationId: TestCorrelationId, causationId: TestCausationId);

        ledger.SustainabilityTargets.Should().ContainSingle().Which.Should().BeSameAs(target);
        ledger.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<SustainabilityTargetSetEvent>();
        target.Status.Should().Be(SustainabilityTargetStatus.OnTrack);
        target.CurrentValue.Should().Be(40m);
    }

    [Fact]
    public void UpdateTargetProgress_WhenTargetTrendsBehindPlan_RaisesAtRiskEventOnce()
    {
        var ledger = OpenLedger();
        var setAt = FixedTimestamp;
        var targetDate = UtcTimestamp.From(FixedUtcNow.AddDays(100));

        var target = ledger.SetSustainabilityTarget(
            SustainabilityTargetType.WasteDiversionRate, baselineValue: 0m, targetValue: 100m,
            targetDateUtc: targetDate, setAtUtc: setAt, setBy: TestUserId, actor: TestActor,
            correlationId: TestCorrelationId, causationId: TestCausationId);

        // Half the time has elapsed but almost none of the progress has been made -> AtRisk.
        var midpoint = UtcTimestamp.From(FixedUtcNow.AddDays(50));
        ledger.UpdateTargetProgress(target.SustainabilityTargetId, currentValue: 1m, atUtc: midpoint,
            higherIsBetter: true, actor: TestActor, correlationId: TestCorrelationId, causationId: TestCausationId);

        target.Status.Should().Be(SustainabilityTargetStatus.AtRisk);
        ledger.DomainEvents.OfType<SustainabilityTargetAtRiskEvent>().Should().ContainSingle();

        // A second update that keeps the target at-risk must NOT raise a duplicate event.
        var later = UtcTimestamp.From(FixedUtcNow.AddDays(60));
        ledger.UpdateTargetProgress(target.SustainabilityTargetId, currentValue: 2m, atUtc: later,
            higherIsBetter: true, actor: TestActor, correlationId: TestCorrelationId, causationId: TestCausationId);

        ledger.DomainEvents.OfType<SustainabilityTargetAtRiskEvent>().Should().ContainSingle(
            "the at-risk event is a transition hook, not re-raised on every subsequent at-risk update");
    }

    [Fact]
    public void UpdateTargetProgress_ForUnknownTargetId_Throws()
    {
        var ledger = OpenLedger();
        var unknownId = PlantSafety.Domain.Sustainability.AdvancedMetrics.ValueObjects.Ids.SustainabilityTargetId.New();

        var act = () => ledger.UpdateTargetProgress(unknownId, 10m, FixedTimestamp, true, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<ArgumentException>().WithParameterName("sustainabilityTargetId");
    }

    // ---- Close / Archive -------------------------------------------------

    [Fact]
    public void Close_WhenOpen_TransitionsToClosed()
    {
        var ledger = OpenLedger();

        ledger.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        ledger.Status.Should().Be(AdvancedMetricsLedgerStatus.Closed);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ThrowsInvalidTransition()
    {
        var ledger = OpenLedger();
        ledger.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => ledger.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<AdvancedMetricsInvalidStatusTransitionException>();
    }

    [Fact]
    public void Archive_WhenClosed_TransitionsToArchived()
    {
        var ledger = OpenLedger();
        ledger.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        ledger.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        ledger.Status.Should().Be(AdvancedMetricsLedgerStatus.Archived);
    }

    [Fact]
    public void Archive_WhenStillOpen_ThrowsInvalidTransition()
    {
        var ledger = OpenLedger();

        var act = () => ledger.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<AdvancedMetricsInvalidStatusTransitionException>(
            "a ledger must be Closed before it can be Archived -- Open -> Archived is not a permitted transition");
    }

    // ---- ClearDomainEvents -------------------------------------------------

    [Fact]
    public void ClearDomainEvents_RemovesAllPendingEvents()
    {
        var ledger = OpenLedger();
        ledger.RecordCarbonEmission(GhgScope.Scope1, "x", 1m, "v1", FixedTimestamp, FixedTimestamp, TestUserId, TestActor, TestCorrelationId, TestCausationId);

        ledger.ClearDomainEvents();

        ledger.DomainEvents.Should().BeEmpty();
    }
}
