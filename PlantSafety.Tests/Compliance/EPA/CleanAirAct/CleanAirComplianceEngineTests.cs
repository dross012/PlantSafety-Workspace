// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.EPA.CleanAirAct
//  Layer:          Tests (Unit)
//  File:           CleanAirComplianceEngineTests.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the CleanAirComplianceEngine domain service.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.EPA.CleanAirAct.Enums;
using PlantSafety.Domain.Compliance.EPA.CleanAirAct.Services;

namespace PlantSafety.Tests.Compliance.EPA.CleanAirAct;

/// <summary>Unit tests for <see cref="CleanAirComplianceEngine"/>.</summary>
public sealed class CleanAirComplianceEngineTests : UnitTestBase
{
    private readonly CleanAirComplianceEngine _engine = new();

    [Fact]
    public void EvaluateCompliance_MajorSourceWithoutTitleVPermit_ShouldReportCriticalViolation()
    {
        var input = new CleanAirComplianceInput(
            EmissionSources:
            [
                new EmissionSourceRecord(
                    SourceName: "Boiler 1",
                    AnyPollutantTonsPerYear: 150m,
                    SingleHapTonsPerYear: 2m,
                    CombinedHapTonsPerYear: 4m,
                    IsSubjectToNsps: false,
                    IsSubjectToNeshap: false,
                    HasTitleVPermit: false,
                    TitleVPermitExpiresAtUtc: null),
            ],
            EvaluatedAtUtc: FixedTimestamp);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().ContainSingle(v => v.Severity == "Critical" && v.Citation.Contains("Title V"));
    }

    [Fact]
    public void EvaluateCompliance_NonMajorSourceNoNspsNeshap_ShouldReportNoViolations()
    {
        var input = new CleanAirComplianceInput(
            EmissionSources:
            [
                new EmissionSourceRecord(
                    SourceName: "Small Generator",
                    AnyPollutantTonsPerYear: 5m,
                    SingleHapTonsPerYear: 1m,
                    CombinedHapTonsPerYear: 2m,
                    IsSubjectToNsps: false,
                    IsSubjectToNeshap: false,
                    HasTitleVPermit: false,
                    TitleVPermitExpiresAtUtc: null),
            ],
            EvaluatedAtUtc: FixedTimestamp);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.IsCompliant.Should().BeTrue();
        result.Value.Violations.Should().BeEmpty();
    }

    [Fact]
    public void ClassifySourceCategory_AboveAnyThreshold_ShouldReturnMajorSource()
    {
        var source = new EmissionSourceRecord(
            SourceName: "Kiln", AnyPollutantTonsPerYear: 0m, SingleHapTonsPerYear: 10m, CombinedHapTonsPerYear: 0m,
            IsSubjectToNsps: false, IsSubjectToNeshap: false, HasTitleVPermit: true, TitleVPermitExpiresAtUtc: null);

        CleanAirComplianceEngine.ClassifySourceCategory(source).Should().Be(CaaSourceCategory.MajorSource);
    }

    [Fact]
    public void EvaluateCompliance_ExpiredTitleVPermit_ShouldReportCriticalViolation()
    {
        var input = new CleanAirComplianceInput(
            EmissionSources:
            [
                new EmissionSourceRecord(
                    SourceName: "Furnace 2",
                    AnyPollutantTonsPerYear: 200m,
                    SingleHapTonsPerYear: 0m,
                    CombinedHapTonsPerYear: 0m,
                    IsSubjectToNsps: false,
                    IsSubjectToNeshap: false,
                    HasTitleVPermit: true,
                    TitleVPermitExpiresAtUtc: UtcTimestamp.From(FixedUtcNow.AddDays(-10))),
            ],
            EvaluatedAtUtc: FixedTimestamp);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().ContainSingle(v => v.Severity == "Critical");
    }

    [Fact]
    public void EvaluateCompliance_NegativeTonnage_ShouldFail()
    {
        var input = new CleanAirComplianceInput(
            EmissionSources:
            [
                new EmissionSourceRecord(
                    SourceName: "Bad Data Source",
                    AnyPollutantTonsPerYear: -1m,
                    SingleHapTonsPerYear: 0m,
                    CombinedHapTonsPerYear: 0m,
                    IsSubjectToNsps: false,
                    IsSubjectToNeshap: false,
                    HasTitleVPermit: false,
                    TitleVPermitExpiresAtUtc: null),
            ],
            EvaluatedAtUtc: FixedTimestamp);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeFalse();
    }
}
