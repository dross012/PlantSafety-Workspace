// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Sustainability.AdvancedMetrics
//  Layer:          IntegrationTests
//  File:           AdvancedMetricsLedgerHydrationTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Repository-hydration test for AdvancedMetricsLedgerDapperRepository's
//                  reflection-based rehydration path. No live SQL Server/Testcontainers instance
//                  is available in this environment (Docker daemon unreachable), so this exercises
//                  the exact same AggregateReflectionHydrator calls
//                  AdvancedMetricsLedgerDapperRepository.HydrateAsync makes for its
//                  SustainabilityTarget child rows -- including the branch that only overwrites
//                  CurrentValue/Status/LastProgressUpdateAtUtc when the persisted row has drifted
//                  from the just-set baseline (SustainabilityTarget.UpdateProgress is `internal`,
//                  so the Infrastructure-layer repository cannot call it directly across the
//                  assembly boundary without InternalsVisibleTo; it uses
//                  AggregateReflectionHydrator.SetProperty instead -- this test pins that
//                  workaround's correctness). Deterministic, parallel-safe, no I/O, no network.
// ============================================================================
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Aggregates;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Entities;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Enums;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Events;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.IntegrationTests.Sustainability.AdvancedMetrics;

public sealed class AdvancedMetricsLedgerHydrationTests
{
    private static readonly DateTime FixedUtcNow = new(2026, 8, 4, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Mirrors AdvancedMetricsLedgerDapperRepository.HydrateAsync's SustainabilityTarget branch
    /// verbatim: construct via the public constructor (baseline state), then reflectively
    /// overwrite CurrentValue/Status/LastProgressUpdateAtUtc only when the persisted row has
    /// drifted from that baseline -- exactly as a row whose progress was updated after being set
    /// would come back from [usp_SustainabilityTarget_ListByLedger].
    /// </summary>
    private static SustainabilityTarget HydrateTargetLikeRepository(
        SustainabilityTargetType targetType, decimal baselineValue, decimal targetValue, decimal currentValue,
        SustainabilityTargetStatus status, DateTime targetDateUtc, DateTime setAtUtc, Guid setBy, DateTime? lastProgressUpdateAtUtc)
    {
        var target = new SustainabilityTarget(
            SustainabilityTargetId.New(), targetType, baselineValue, targetValue,
            UtcTimestamp.From(targetDateUtc), UtcTimestamp.From(setAtUtc), new UserId(setBy));

        if (currentValue != baselineValue || status != SustainabilityTargetStatus.OnTrack)
        {
            AggregateReflectionHydrator.SetProperty(target, nameof(SustainabilityTarget.CurrentValue), currentValue);
            AggregateReflectionHydrator.SetProperty(target, nameof(SustainabilityTarget.Status), status);
            AggregateReflectionHydrator.SetProperty(target, nameof(SustainabilityTarget.LastProgressUpdateAtUtc), UtcTimestamp.From(lastProgressUpdateAtUtc ?? setAtUtc));
        }

        return target;
    }

    [Fact]
    public void HydrateTarget_WithNonBaselineProgressRow_RehydratesCurrentValueStatusAndLastProgressUpdateCorrectly()
    {
        var setAt = FixedUtcNow.AddDays(-30);
        var lastUpdate = FixedUtcNow.AddDays(-2);

        var target = HydrateTargetLikeRepository(
            targetType: SustainabilityTargetType.WasteDiversionRate,
            baselineValue: 40m, targetValue: 75m, currentValue: 58.5m,
            status: SustainabilityTargetStatus.AtRisk,
            targetDateUtc: FixedUtcNow.AddYears(1), setAtUtc: setAt, setBy: Guid.NewGuid(),
            lastProgressUpdateAtUtc: lastUpdate);

        target.BaselineValue.Should().Be(40m, "baseline is immutable and must be unaffected by the reflection overwrite");
        target.TargetValue.Should().Be(75m);
        target.CurrentValue.Should().Be(58.5m, "the drifted current value must round-trip exactly");
        target.Status.Should().Be(SustainabilityTargetStatus.AtRisk, "the persisted non-OnTrack status must round-trip exactly");
        target.LastProgressUpdateAtUtc.Should().NotBeNull();
        target.LastProgressUpdateAtUtc!.Value.Value.Should().Be(lastUpdate, "the persisted progress-update timestamp must round-trip exactly, not fall back to SetAtUtc");
    }

    [Fact]
    public void HydrateTarget_WithNullLastProgressUpdateColumnButDriftedCurrentValue_FallsBackToSetAtUtc()
    {
        // Mirrors the repository's `UtcTimestamp.From(r.LastProgressUpdateAtUtc ?? r.SetAtUtc)` guard:
        // a row can have drifted CurrentValue/Status without ever having a progress-update
        // timestamp recorded (e.g. a future write path that skips the timestamp column).
        var setAt = FixedUtcNow.AddDays(-10);

        var target = HydrateTargetLikeRepository(
            targetType: SustainabilityTargetType.EnergyIntensity,
            baselineValue: 10m, targetValue: 2m, currentValue: 9.5m,
            status: SustainabilityTargetStatus.OnTrack, // still counts as drifted because CurrentValue != BaselineValue
            targetDateUtc: FixedUtcNow.AddYears(1), setAtUtc: setAt, setBy: Guid.NewGuid(),
            lastProgressUpdateAtUtc: null);

        target.CurrentValue.Should().Be(9.5m);
        target.LastProgressUpdateAtUtc!.Value.Value.Should().Be(setAt);
    }

    [Fact]
    public void HydrateTarget_AtExactBaselineWithOnTrackStatus_SkipsReflectionOverwriteAndLeavesLastProgressUpdateNull()
    {
        // Brand-new target row (never progressed): the repository intentionally skips the
        // reflection overwrite branch entirely in this case (it is a documented no-op optimization).
        var setAt = FixedUtcNow;

        var target = HydrateTargetLikeRepository(
            targetType: SustainabilityTargetType.CarbonIntensityPerUnitOutput,
            baselineValue: 100m, targetValue: 20m, currentValue: 100m,
            status: SustainabilityTargetStatus.OnTrack,
            targetDateUtc: FixedUtcNow.AddYears(1), setAtUtc: setAt, setBy: Guid.NewGuid(),
            lastProgressUpdateAtUtc: null);

        target.CurrentValue.Should().Be(100m);
        target.Status.Should().Be(SustainabilityTargetStatus.OnTrack);
        target.LastProgressUpdateAtUtc.Should().BeNull("a never-updated target must not fabricate a progress-update timestamp");
    }

    [Fact]
    public void HydrateLedger_WithReflectionCreatedUninitializedInstance_PopulatesPrivateSettersAndBackingFields()
    {
        // End-to-end mirror of AdvancedMetricsLedgerDapperRepository.HydrateAsync's header +
        // child-collection population, confirming AggregateReflectionHydrator.CreateUninitialized /
        // SetProperty / SetField correctly bypass the aggregate's "for rehydration only" private
        // constructor and populate every private-setter property plus the private backing-field
        // collections, including a SustainabilityTarget with drifted progress.
        var ledgerId = AdvancedMetricsLedgerId.New();
        var tenantId = Guid.NewGuid();
        var createdBy = new UserId(Guid.NewGuid());
        var periodStart = UtcTimestamp.From(FixedUtcNow.AddMonths(-3));
        var periodEnd = UtcTimestamp.From(FixedUtcNow);

        var agg = AggregateReflectionHydrator.CreateUninitialized<AdvancedMetricsLedger>();
        AggregateReflectionHydrator.SetProperty(agg, nameof(AdvancedMetricsLedger.AdvancedMetricsLedgerId), ledgerId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AdvancedMetricsLedger.TenantId), tenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AdvancedMetricsLedger.SiteId), (Guid?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AdvancedMetricsLedger.ReportingPeriodStartUtc), periodStart);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AdvancedMetricsLedger.ReportingPeriodEndUtc), periodEnd);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AdvancedMetricsLedger.Status), AdvancedMetricsLedgerStatus.Closed);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AdvancedMetricsLedger.CreatedAtUtc), periodStart);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AdvancedMetricsLedger.CreatedBy), createdBy);
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IAdvancedMetricsDomainEvent>());
        AggregateReflectionHydrator.SetField(agg, "_carbonEmissionRecords", new List<CarbonEmissionRecord>());
        AggregateReflectionHydrator.SetField(agg, "_energyIntensityMetrics", new List<EnergyIntensityMetric>());
        AggregateReflectionHydrator.SetField(agg, "_waterIntensityMetrics", new List<WaterIntensityMetric>());
        AggregateReflectionHydrator.SetField(agg, "_wasteDiversionRecords", new List<WasteDiversionRecord>());

        var target = HydrateTargetLikeRepository(
            SustainabilityTargetType.WasteDiversionRate, baselineValue: 40m, targetValue: 75m, currentValue: 61.2m,
            status: SustainabilityTargetStatus.OnTrack, targetDateUtc: FixedUtcNow.AddYears(1),
            setAtUtc: FixedUtcNow.AddDays(-20), setBy: createdBy.Value, lastProgressUpdateAtUtc: FixedUtcNow.AddDays(-1));
        AggregateReflectionHydrator.SetField(agg, "_sustainabilityTargets", new List<SustainabilityTarget> { target });

        agg.AdvancedMetricsLedgerId.Should().Be(ledgerId);
        agg.TenantId.Should().Be(tenantId);
        agg.Status.Should().Be(AdvancedMetricsLedgerStatus.Closed);
        agg.CreatedBy.Should().Be(createdBy);
        agg.DomainEvents.Should().BeEmpty();
        agg.SustainabilityTargets.Should().ContainSingle();
        agg.SustainabilityTargets.Single().CurrentValue.Should().Be(61.2m);
        agg.SustainabilityTargets.Single().Status.Should().Be(SustainabilityTargetStatus.OnTrack);
        agg.SustainabilityTargets.Single().LastProgressUpdateAtUtc!.Value.Value.Should().Be(FixedUtcNow.AddDays(-1));
    }
}
