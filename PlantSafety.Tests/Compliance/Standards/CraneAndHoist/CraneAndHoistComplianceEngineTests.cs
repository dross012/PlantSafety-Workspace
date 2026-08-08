// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Standards.CraneAndHoist
//  Layer:          Tests (Unit)
//  File:           CraneAndHoistComplianceEngineTests.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the CraneAndHoistComplianceEngine domain service.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.Standards.CraneAndHoist.Services;
using PlantSafety.Domain.Compliance.Standards.CraneAndHoist.ValueObjects;

namespace PlantSafety.Tests.Compliance.Standards.CraneAndHoist;

/// <summary>Unit tests for <see cref="CraneAndHoistComplianceEngine"/>.</summary>
public sealed class CraneAndHoistComplianceEngineTests : UnitTestBase
{
    private readonly CraneAndHoistComplianceEngine _engine = new();

    private static CraneHoistAssetSnapshot CompliantAsset(UtcTimestamp now, string tag = "CRANE-001") => new(
        AssetTag: tag,
        AssetTitle: "Bay 3 Overhead Bridge Crane",
        AssetType: "OverheadBridgeCrane",
        RatedLoadCapacityKg: 5_000m,
        LastObservedLoadKg: 2_000m,
        LastLiftAtUtc: new UtcTimestamp(now.Value.AddDays(-1)),
        OverloadEventCount: 0,
        OverloadFlagged: false,
        InspectionDueAtUtc: new UtcTimestamp(now.Value.AddDays(60)),
        LastInspectionAtUtc: new UtcTimestamp(now.Value.AddDays(-30)),
        LastInspectionPassed: true,
        OperatorQualifiedAtUtc: new UtcTimestamp(now.Value.AddYears(-1)),
        OperatorQualificationExpiresAtUtc: new UtcTimestamp(now.Value.AddYears(1)),
        OperatorQualificationSuspended: false,
        LastRiggingInspectionAtUtc: new UtcTimestamp(now.Value.AddDays(-10)),
        LastRiggingDefectDetected: false,
        Status: "InService");

    [Fact]
    public void EvaluateCompliance_AllCompliant_ShouldReportNoFindings()
    {
        var now = UtcTimestamp.Now();
        var input = new CraneAndHoistComplianceInput([CompliantAsset(now)], now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeTrue();
        result.Value.Violations.Should().BeEmpty();
        result.Value.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void EvaluateCompliance_PastInspectionDeadline_ShouldReportCriticalViolation()
    {
        var now = UtcTimestamp.Now();
        var asset = CompliantAsset(now) with { InspectionDueAtUtc = new UtcTimestamp(now.Value.AddDays(-5)) };
        var input = new CraneAndHoistComplianceInput([asset], now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().ContainSingle(v =>
            v.Severity == "Critical" && v.Citation == "ASME B30 periodic inspection interval");
    }

    [Fact]
    public void EvaluateCompliance_OverloadedCrane_ShouldReportCriticalViolation()
    {
        var now = UtcTimestamp.Now();
        var asset = CompliantAsset(now) with
        {
            OverloadEventCount = 2,
            LastObservedLoadKg = 6_500m,
            OverloadFlagged = true,
        };
        var input = new CraneAndHoistComplianceInput([asset], now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Violations.Should().ContainSingle(v =>
            v.Severity == "Critical" && v.Citation == "ASME B30 rated load capacity");
    }

    [Fact]
    public void EvaluateCompliance_ExpiredOperatorQualification_ShouldReportHighViolation()
    {
        var now = UtcTimestamp.Now();
        var asset = CompliantAsset(now) with
        {
            OperatorQualificationExpiresAtUtc = new UtcTimestamp(now.Value.AddDays(-1)),
        };
        var input = new CraneAndHoistComplianceInput([asset], now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Violations.Should().ContainSingle(v =>
            v.Severity == "High" && v.Citation == "ASME B30 qualified/designated operator requirement");
    }

    [Fact]
    public void EvaluateCompliance_FailedLastInspection_ShouldReportCriticalViolation()
    {
        var now = UtcTimestamp.Now();
        var asset = CompliantAsset(now) with { LastInspectionPassed = false };
        var input = new CraneAndHoistComplianceInput([asset], now);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.Violations.Should().ContainSingle(v =>
            v.Severity == "Critical" && v.Citation == "ASME B30.2/B30.10 inspection deficiency");
    }

    [Fact]
    public void EvaluateCompliance_RiggingDefectDetected_ShouldReportHighViolation()
    {
        var now = UtcTimestamp.Now();
        var asset = CompliantAsset(now) with { LastRiggingDefectDetected = true };
        var input = new CraneAndHoistComplianceInput([asset], now);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.Violations.Should().ContainSingle(v =>
            v.Severity == "High" && v.Citation == "ASME B30.9/B30.10 rigging/hook inspection");
    }

    [Fact]
    public void EvaluateCompliance_OperatorQualificationSuspended_ShouldReportHighViolation()
    {
        var now = UtcTimestamp.Now();
        var asset = CompliantAsset(now) with { OperatorQualificationSuspended = true };
        var input = new CraneAndHoistComplianceInput([asset], now);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.Violations.Should().ContainSingle(v =>
            v.Severity == "High" && v.Citation == "ASME B30 qualified/designated operator requirement");
    }

    [Fact]
    public void EvaluateCompliance_InspectionApproachingDueWithin14Days_ShouldReportMediumWarning()
    {
        var now = UtcTimestamp.Now();
        var asset = CompliantAsset(now) with { InspectionDueAtUtc = new UtcTimestamp(now.Value.AddDays(5)) };
        var input = new CraneAndHoistComplianceInput([asset], now);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.Violations.Should().BeEmpty();
        result.Value.Warnings.Should().ContainSingle(w => w.Severity == "Medium");
    }

    [Fact]
    public void EvaluateCompliance_NegativeRatedCapacity_ShouldFail()
    {
        var now = UtcTimestamp.Now();
        var asset = CompliantAsset(now) with { RatedLoadCapacityKg = -1m };
        var input = new CraneAndHoistComplianceInput([asset], now);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }
}
