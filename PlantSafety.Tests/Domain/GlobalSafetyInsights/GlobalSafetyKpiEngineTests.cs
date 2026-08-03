// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.GlobalSafetyInsights
//  Layer:          Tests (Unit)
//  File:           GlobalSafetyKpiEngineTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GlobalSafetyKpiEngine: OSHA-standard TRIR/LTIR formula
//                  verification, exposure rate averaging, incident density, and machine-state
//                  correlation. Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Services;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;
using PlantSafety.Tests.Common.TestBase;

namespace PlantSafety.Tests.Domain.GlobalSafetyInsights;

/// <summary>Unit tests for <see cref="GlobalSafetyKpiEngine"/>.</summary>
public sealed class GlobalSafetyKpiEngineTests : UnitTestBase
{
    private readonly GlobalSafetyKpiEngine _sut = new();

    private static SafetyTelemetryEvent Telemetry(
        Guid siteId, int recordable = 0, int lostTime = 0, decimal? hours = null,
        decimal? exposure = null, bool? degraded = null, DateTime? occurredAtUtc = null) => new()
    {
        TenantId = TestTenantId.Value,
        SiteId = siteId,
        SourceModule = "Test",
        OccurredAtUtc = UtcTimestamp.From(occurredAtUtc ?? FixedUtcNow),
        RecordableIncidentCount = recordable,
        LostTimeIncidentCount = lostTime,
        HoursWorkedContribution = hours,
        ExposureLevelScore = exposure,
        MachineStateDegraded = degraded,
    };

    [Fact]
    public void Compute_WithNoTelemetry_ReturnsZeroedSnapshot()
    {
        var result = _sut.Compute([]);

        result.Trir.Should().Be(0m);
        result.Ltir.Should().Be(0m);
        result.ExposureRate.Should().Be(0m);
        result.IncidentDensity.Should().Be(0m);
        result.MachineStateIncidentCorrelation.Should().Be(0.0);
    }

    [Fact]
    public void Compute_Trir_UsesOshaStandardFormula()
    {
        var siteId = Guid.NewGuid();
        // 1 recordable incident, 200,000 hours worked -> TRIR = 1 x 200,000 / 200,000 = 1.0
        var telemetry = new[]
        {
            Telemetry(siteId, recordable: 1),
            Telemetry(siteId, hours: 200_000m),
        };

        var result = _sut.Compute(telemetry);

        result.Trir.Should().Be(1.0m);
        result.TotalRecordableIncidents.Should().Be(1);
        result.TotalHoursWorked.Should().Be(200_000m);
    }

    [Fact]
    public void Compute_Ltir_UsesOshaStandardFormula()
    {
        var siteId = Guid.NewGuid();
        // 2 lost-time incidents, 400,000 hours worked -> LTIR = 2 x 200,000 / 400,000 = 1.0
        var telemetry = new[]
        {
            Telemetry(siteId, lostTime: 1),
            Telemetry(siteId, lostTime: 1),
            Telemetry(siteId, hours: 400_000m),
        };

        var result = _sut.Compute(telemetry);

        result.Ltir.Should().Be(1.0m);
        result.TotalLostTimeIncidents.Should().Be(2);
    }

    [Fact]
    public void Compute_WithZeroHoursWorked_TrirAndLtirAreZero_NotDivideByZero()
    {
        var siteId = Guid.NewGuid();
        var telemetry = new[] { Telemetry(siteId, recordable: 3, lostTime: 1) };

        var result = _sut.Compute(telemetry);

        result.Trir.Should().Be(0m);
        result.Ltir.Should().Be(0m);
    }

    [Fact]
    public void Compute_ExposureRate_AveragesOnlyRecordsWithExposureScores()
    {
        var siteId = Guid.NewGuid();
        var telemetry = new[]
        {
            Telemetry(siteId, exposure: 10m),
            Telemetry(siteId, exposure: 20m),
            Telemetry(siteId, recordable: 1), // no exposure score -- excluded from the average
        };

        var result = _sut.Compute(telemetry);

        result.ExposureRate.Should().Be(15m);
    }

    [Fact]
    public void Compute_MachineStateIncidentCorrelation_PositiveWhenDegradedSitesHaveMoreIncidents()
    {
        var siteA = Guid.NewGuid();
        var siteB = Guid.NewGuid();
        var siteC = Guid.NewGuid();

        var telemetry = new[]
        {
            Telemetry(siteA, recordable: 5, degraded: true),
            Telemetry(siteA, degraded: true),
            Telemetry(siteB, recordable: 1, degraded: false),
            Telemetry(siteC, recordable: 0, degraded: false),
        };

        var result = _sut.Compute(telemetry);

        result.MachineStateIncidentCorrelation.Should().BeGreaterThan(0.0);
    }

    [Fact]
    public void Compute_MachineStateIncidentCorrelation_ZeroWithFewerThanTwoSites()
    {
        var siteId = Guid.NewGuid();
        var telemetry = new[] { Telemetry(siteId, recordable: 5, degraded: true) };

        var result = _sut.Compute(telemetry);

        result.MachineStateIncidentCorrelation.Should().Be(0.0);
    }
}
