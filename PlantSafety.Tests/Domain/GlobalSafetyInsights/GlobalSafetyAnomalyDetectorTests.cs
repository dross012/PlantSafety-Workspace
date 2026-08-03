// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.GlobalSafetyInsights
//  Layer:          Tests (Unit)
//  File:           GlobalSafetyAnomalyDetectorTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GlobalSafetyAnomalyDetector: z-score cross-site incident-spike
//                  detection, recurring exposure-exceedance detection, and systemic machine-line
//                  failure detection. Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Enums;
using PlantSafety.Domain.GlobalSafetyInsights.Services;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;
using PlantSafety.Tests.Common.TestBase;

namespace PlantSafety.Tests.Domain.GlobalSafetyInsights;

/// <summary>Unit tests for <see cref="GlobalSafetyAnomalyDetector"/>.</summary>
public sealed class GlobalSafetyAnomalyDetectorTests : UnitTestBase
{
    private readonly GlobalSafetyAnomalyDetector _sut = new();

    private static SafetyTelemetryEvent Telemetry(
        Guid siteId, int recordable = 0, decimal? exposure = null, string? machineFamily = null, bool? degraded = null) => new()
    {
        TenantId = TestTenantId.Value,
        SiteId = siteId,
        SourceModule = "Test",
        OccurredAtUtc = FixedTimestamp,
        RecordableIncidentCount = recordable,
        ExposureLevelScore = exposure,
        MachineFamily = machineFamily,
        MachineStateDegraded = degraded,
    };

    [Fact]
    public void DetectAnomalies_WithFewerThanThreeSites_DetectsNoIncidentSpike()
    {
        var telemetry = new[]
        {
            Telemetry(Guid.NewGuid(), recordable: 1),
            Telemetry(Guid.NewGuid(), recordable: 50),
        };

        var result = _sut.DetectAnomalies(telemetry, FixedTimestamp);

        result.Should().NotContain(a => a.Kind == GlobalSafetyAnomalyKind.CrossSiteIncidentSpike);
    }

    [Fact]
    public void DetectAnomalies_OutlierSite_DetectsCrossSiteIncidentSpike()
    {
        // z-score = (outlier - mean) / stddev; with 5 baseline sites at count=1 and one outlier at
        // count=20 (20x baseline), z = 2.236, comfortably above the ZScoreThreshold of 2.0.
        var outlierSite = Guid.NewGuid();
        var telemetry = new[]
        {
            Telemetry(Guid.NewGuid(), recordable: 1),
            Telemetry(Guid.NewGuid(), recordable: 1),
            Telemetry(Guid.NewGuid(), recordable: 1),
            Telemetry(Guid.NewGuid(), recordable: 1),
            Telemetry(Guid.NewGuid(), recordable: 1),
            Telemetry(outlierSite, recordable: 20),
        };

        var result = _sut.DetectAnomalies(telemetry, FixedTimestamp);

        result.Should().ContainSingle(a => a.Kind == GlobalSafetyAnomalyKind.CrossSiteIncidentSpike && a.SiteId == outlierSite);
    }

    [Fact]
    public void DetectAnomalies_UniformIncidentCounts_DetectsNoSpike()
    {
        var telemetry = new[]
        {
            Telemetry(Guid.NewGuid(), recordable: 2),
            Telemetry(Guid.NewGuid(), recordable: 2),
            Telemetry(Guid.NewGuid(), recordable: 2),
            Telemetry(Guid.NewGuid(), recordable: 2),
        };

        var result = _sut.DetectAnomalies(telemetry, FixedTimestamp);

        result.Should().NotContain(a => a.Kind == GlobalSafetyAnomalyKind.CrossSiteIncidentSpike);
    }

    [Fact]
    public void DetectAnomalies_RecurringElevatedExposure_DetectsRecurringExposureExceedance()
    {
        // Same z-score shape as the incident-spike test above: 5 baseline sites averaging 5, one
        // outlier site averaging 100 (20x baseline) -> z = 2.236, above the ZScoreThreshold of 2.0.
        var outlierSite = Guid.NewGuid();
        var telemetry = new[]
        {
            Telemetry(Guid.NewGuid(), exposure: 5m),
            Telemetry(Guid.NewGuid(), exposure: 5m),
            Telemetry(Guid.NewGuid(), exposure: 5m),
            Telemetry(Guid.NewGuid(), exposure: 5m),
            Telemetry(Guid.NewGuid(), exposure: 5m),
            Telemetry(outlierSite, exposure: 95m),
            Telemetry(outlierSite, exposure: 105m),
        };

        var result = _sut.DetectAnomalies(telemetry, FixedTimestamp);

        result.Should().ContainSingle(a => a.Kind == GlobalSafetyAnomalyKind.RecurringExposureExceedance && a.SiteId == outlierSite);
    }

    [Fact]
    public void DetectAnomalies_SingleElevatedExposureReading_IsNotRecurring_NoAnomaly()
    {
        var outlierSite = Guid.NewGuid();
        var telemetry = new[]
        {
            Telemetry(Guid.NewGuid(), exposure: 5m),
            Telemetry(Guid.NewGuid(), exposure: 5m),
            Telemetry(Guid.NewGuid(), exposure: 5m),
            Telemetry(outlierSite, exposure: 100m), // only one reading -- not "recurring"
        };

        var result = _sut.DetectAnomalies(telemetry, FixedTimestamp);

        result.Should().NotContain(a => a.Kind == GlobalSafetyAnomalyKind.RecurringExposureExceedance);
    }

    [Fact]
    public void DetectAnomalies_DegradedMachineFamilyAcrossMultipleSites_DetectsSystemicMachineLineFailure()
    {
        var siteA = Guid.NewGuid();
        var siteB = Guid.NewGuid();
        var telemetry = new[]
        {
            Telemetry(siteA, machineFamily: "Conveyor-X200", degraded: true),
            Telemetry(siteB, machineFamily: "Conveyor-X200", degraded: true),
        };

        var result = _sut.DetectAnomalies(telemetry, FixedTimestamp);

        result.Should().ContainSingle(a => a.Kind == GlobalSafetyAnomalyKind.SystemicMachineLineFailure && a.MachineFamily == "Conveyor-X200");
    }

    [Fact]
    public void DetectAnomalies_DegradedMachineFamilyAtSingleSite_IsNotSystemic_NoAnomaly()
    {
        var siteA = Guid.NewGuid();
        var telemetry = new[]
        {
            Telemetry(siteA, machineFamily: "Conveyor-X200", degraded: true),
            Telemetry(siteA, machineFamily: "Conveyor-X200", degraded: true),
        };

        var result = _sut.DetectAnomalies(telemetry, FixedTimestamp);

        result.Should().NotContain(a => a.Kind == GlobalSafetyAnomalyKind.SystemicMachineLineFailure);
    }
}
