// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Sustainability.AdvancedMetrics.Domain
//  Layer:          Tests (Unit)
//  File:           SustainabilityTargetTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Invariant tests for the SustainabilityTarget child entity and the
//                  SustainabilityTargetRiskPolicy that governs its progress-status transitions
//                  (OnTrack / AtRisk / Achieved / Missed). Isolated, deterministic, <50ms per
//                  test. No I/O, no network.
// ============================================================================
using System.Reflection;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Entities;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Enums;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Policies;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.ValueObjects.Ids;

namespace PlantSafety.Tests.Sustainability.AdvancedMetrics.Domain;

public sealed class SustainabilityTargetTests : UnitTestBase
{
    private SustainabilityTarget NewTarget(decimal baseline, decimal target, UtcTimestamp setAt, UtcTimestamp dueAt) =>
        new(SustainabilityTargetId.New(), SustainabilityTargetType.WasteDiversionRate, baseline, target, dueAt, setAt, TestUserId);

    /// <summary>
    /// SustainabilityTarget.UpdateProgress is intentionally `internal` -- only
    /// AdvancedMetricsLedger.UpdateTargetProgress (same assembly) may call it; there is no
    /// InternalsVisibleTo grant to the Tests assembly (see also
    /// AdvancedMetricsLedgerDapperRepository.cs, which hits the same boundary from
    /// Infrastructure and works around it via AggregateReflectionHydrator instead). This helper
    /// invokes the internal member via reflection so this entity-level test file can still pin
    /// its return-value/field-mutation contract directly, independent of the aggregate-level
    /// coverage in AdvancedMetricsLedgerTests.UpdateTargetProgress_*.
    /// </summary>
    private static bool InvokeUpdateProgress(SustainabilityTarget target, decimal currentValue, SustainabilityTargetStatus newStatus, UtcTimestamp atUtc)
    {
        var method = typeof(SustainabilityTarget).GetMethod(
            "UpdateProgress", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("SustainabilityTarget.UpdateProgress method not found via reflection.");

        return (bool)method.Invoke(target, [currentValue, newStatus, atUtc])!;
    }

    [Fact]
    public void Constructor_InitializesCurrentValueToBaselineAndStatusOnTrack()
    {
        var target = NewTarget(40m, 75m, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddYears(1)));

        target.CurrentValue.Should().Be(40m);
        target.Status.Should().Be(SustainabilityTargetStatus.OnTrack);
        target.LastProgressUpdateAtUtc.Should().BeNull();
    }

    [Fact]
    public void UpdateProgress_UpdatesTrackedFieldsAndReturnsTrueOnNewAtRiskTransition()
    {
        var target = NewTarget(0m, 100m, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddDays(100)));
        var updateAt = UtcTimestamp.From(FixedUtcNow.AddDays(10));

        var becameAtRisk = InvokeUpdateProgress(target, 5m, SustainabilityTargetStatus.AtRisk, updateAt);

        becameAtRisk.Should().BeTrue();
        target.CurrentValue.Should().Be(5m);
        target.Status.Should().Be(SustainabilityTargetStatus.AtRisk);
        target.LastProgressUpdateAtUtc.Should().Be(updateAt);
    }

    [Fact]
    public void UpdateProgress_WhenAlreadyAtRisk_ReturnsFalseOnRepeatedAtRiskUpdate()
    {
        var target = NewTarget(0m, 100m, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddDays(100)));
        InvokeUpdateProgress(target, 5m, SustainabilityTargetStatus.AtRisk, UtcTimestamp.From(FixedUtcNow.AddDays(10)));

        var becameAtRisk = InvokeUpdateProgress(target, 6m, SustainabilityTargetStatus.AtRisk, UtcTimestamp.From(FixedUtcNow.AddDays(20)));

        becameAtRisk.Should().BeFalse("the entity's own transition guard is what the aggregate uses to decide whether to raise a new event");
    }

    // ---- SustainabilityTargetRiskPolicy -------------------------------------------------

    [Fact]
    public void RiskPolicy_WhenGoalAlreadyMet_ReturnsAchieved()
    {
        var target = NewTarget(0m, 100m, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddDays(100)));

        var status = SustainabilityTargetRiskPolicy.Evaluate(target, currentValue: 100m, nowUtc: UtcTimestamp.From(FixedUtcNow.AddDays(10)), higherIsBetter: true);

        status.Should().Be(SustainabilityTargetStatus.Achieved);
    }

    [Fact]
    public void RiskPolicy_WhenPastDueAndGoalNotMet_ReturnsMissed()
    {
        var target = NewTarget(0m, 100m, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddDays(30)));

        var status = SustainabilityTargetRiskPolicy.Evaluate(target, currentValue: 40m, nowUtc: UtcTimestamp.From(FixedUtcNow.AddDays(31)), higherIsBetter: true);

        status.Should().Be(SustainabilityTargetStatus.Missed);
    }

    [Fact]
    public void RiskPolicy_WhenPacingOnTrack_ReturnsOnTrack()
    {
        var target = NewTarget(0m, 100m, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddDays(100)));

        // Halfway through the window, at (or ahead of) the expected linear-pace value.
        var status = SustainabilityTargetRiskPolicy.Evaluate(target, currentValue: 55m, nowUtc: UtcTimestamp.From(FixedUtcNow.AddDays(50)), higherIsBetter: true);

        status.Should().Be(SustainabilityTargetStatus.OnTrack);
    }

    [Fact]
    public void RiskPolicy_WhenPacingBehindPlan_ReturnsAtRisk()
    {
        var target = NewTarget(0m, 100m, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddDays(100)));

        var status = SustainabilityTargetRiskPolicy.Evaluate(target, currentValue: 5m, nowUtc: UtcTimestamp.From(FixedUtcNow.AddDays(50)), higherIsBetter: true);

        status.Should().Be(SustainabilityTargetStatus.AtRisk);
    }

    [Fact]
    public void RiskPolicy_WithLowerIsBetterDirection_TreatsDecreasingValueAsProgress()
    {
        // Carbon intensity target: baseline 100 -> target 20 (lower is better, the default direction).
        var target = NewTarget(100m, 20m, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddDays(100)));

        var status = SustainabilityTargetRiskPolicy.Evaluate(target, currentValue: 15m, nowUtc: UtcTimestamp.From(FixedUtcNow.AddDays(50)), higherIsBetter: false);

        status.Should().Be(SustainabilityTargetStatus.Achieved, "current (15) is already at/below the lower-is-better target (20)");
    }
}
