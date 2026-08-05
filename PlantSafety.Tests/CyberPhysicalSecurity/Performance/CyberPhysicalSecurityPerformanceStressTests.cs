// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CyberPhysicalSecurity.Performance
//  Layer:          Tests (Unit)
//  File:           CyberPhysicalSecurityPerformanceStressTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Performance + Stress category unit tests scaled to this module's small 5-entity
//                  footprint. Exercises the ControlChangeEvent SHA-256 tamper-evident hash chain
//                  under sustained/bulk load (Stress: long chain construction) and asserts handler
//                  latency stays within the Handbook's generic Command/Query Handler P95 envelope
//                  (Performance & Stress Rules and Design Manual Appendix thresholds: 75/200/400ms
//                  P50/P95/P99 -- applied here as a lightweight guardrail; this manual is scoped to
//                  the Diagnostics Platform, so these are treated as reasonable general-purpose
//                  budgets rather than a certified per-module SLA).
//                  Isolated, deterministic. No I/O, no network.
// ============================================================================
using System.Diagnostics;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;
using ControlChangeEventAggregate = PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Aggregates.ControlChangeEvent;

namespace PlantSafety.Tests.CyberPhysicalSecurity.Performance;

public sealed class CyberPhysicalSecurityPerformanceStressTests : UnitTestBase
{
    // Sized to this module's realistic per-asset control-change volume over a multi-month retention
    // window rather than an arbitrary fixed count -- 5,000 chained records comfortably exceeds
    // realistic per-asset daily-change telemetry rates while staying fast enough for a unit-test
    // budget.
    private const int StressChainLength = 5_000;

    [Fact]
    public void ControlChangeEventHashChain_Under5000SequentialRecords_RemainsFullyVerifiableAndLinear()
    {
        var otAssetId = OtAssetId.New();
        string? priorHash = null;
        var chain = new List<ControlChangeEventAggregate>(StressChainLength);

        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < StressChainLength; i++)
        {
            var record = ControlChangeEventAggregate.Record(
                createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
                causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value, siteId: Guid.NewGuid(),
                otAssetId: otAssetId, changeDescription: $"change #{i}", changedParameter: "SetPoint.Temperature",
                oldValue: i.ToString(), newValue: (i + 1).ToString(),
                detectedAtUtc: UtcTimestamp.From(FixedUtcNow.AddSeconds(i)), priorRecordHash: priorHash);

            chain.Add(record);
            priorHash = record.RecordHash;
        }
        stopwatch.Stop();

        chain.Should().HaveCount(StressChainLength);

        // Walk the full chain and verify every link's PriorRecordHash matches the previous record's
        // RecordHash -- the integrity-sweep pattern a scheduled tamper-detection job would run.
        for (var i = 1; i < chain.Count; i++)
            chain[i].PriorRecordHash.Should().Be(chain[i - 1].RecordHash);

        chain.Select(r => r.RecordHash).Distinct().Should().HaveCount(StressChainLength, "every record's hash must be unique across the chain");

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5_000,
            "constructing a 5,000-record hash chain must stay well within a bulk-ingestion-tolerant budget");
    }

    [Fact]
    public void ControlChangeEventRecord_SingleInvocation_CompletesWithinCommandHandlerP95Budget()
    {
        var stopwatch = Stopwatch.StartNew();

        ControlChangeEventAggregate.Record(
            createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
            causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value, siteId: Guid.NewGuid(),
            otAssetId: OtAssetId.New(), changeDescription: "desc", changedParameter: "SetPoint.Pressure",
            oldValue: "10", newValue: "12", detectedAtUtc: FixedTimestamp, priorRecordHash: null);

        stopwatch.Stop();

        // Generic Command/Query Handler P95 budget (200ms) from the Performance & Stress Rules and
        // Design Manual Appendix; a single SHA-256 hash-chain aggregate construction should complete
        // in well under a millisecond, so this is a generous ceiling rather than a tight assertion.
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200);
    }

    [Fact]
    public void SabotageCorrelationService_Correlate_WithLargeCandidateSet_StaysWithinBudgetAndEscalatesCorrectly()
    {
        // Stress scenario: a burst of near-simultaneous indicators from many sources within the
        // 60-minute correlation window (e.g. a coordinated multi-vector sabotage attempt spanning
        // dozens of OT assets) must still correlate correctly and quickly.
        var newIndicator = PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Aggregates.SabotageIndicator.Raise(
            createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
            causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value, siteId: Guid.NewGuid(),
            source: PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums.SabotageIndicatorSource.ControlChange,
            description: "burst indicator", severity: PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums.SabotageIndicatorSeverity.Medium);

        var candidates = Enumerable.Range(0, 200)
            .Select(i => PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Aggregates.SabotageIndicator.Raise(
                createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
                causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value, siteId: Guid.NewGuid(),
                source: i % 2 == 0
                    ? PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums.SabotageIndicatorSource.SensorIntegrity
                    : PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums.SabotageIndicatorSource.PhysicalAccess,
                description: $"burst indicator #{i}", severity: PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums.SabotageIndicatorSeverity.Medium))
            .ToList();

        var stopwatch = Stopwatch.StartNew();
        var (correlatedCount, autoEscalate) = PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Services.SabotageCorrelationService
            .Correlate(newIndicator, candidates);
        stopwatch.Stop();

        correlatedCount.Should().Be(201);
        autoEscalate.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200);
    }
}
