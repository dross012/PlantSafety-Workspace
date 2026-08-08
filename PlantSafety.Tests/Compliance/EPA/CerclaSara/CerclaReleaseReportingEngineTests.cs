// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.EPA.CerclaSara
//  Layer:          Tests (Unit)
//  File:           CerclaReleaseReportingEngineTests.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the CerclaReleaseReportingEngine domain service.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.EPA.CerclaSara.Services;

namespace PlantSafety.Tests.Compliance.EPA.CerclaSara;

/// <summary>Unit tests for <see cref="CerclaReleaseReportingEngine"/>.</summary>
public sealed class CerclaReleaseReportingEngineTests : UnitTestBase
{
    private readonly CerclaReleaseReportingEngine _engine = new();

    [Fact]
    public void EvaluateReleaseReporting_ReleaseAtOrAboveRq_ShouldReportCriticalViolationsWithNrcGuidance()
    {
        var now = UtcTimestamp.Now();

        var input = new CerclaReleaseInput(
            LinkedIncidentManagementId: Guid.NewGuid(),
            SubstanceName: "Ammonia",
            CasNumber: "7664-41-7",
            ReleasedQuantity: 150m,
            QuantityUnit: "lbs",
            ReportableQuantityThreshold: 100m,
            IsExtremelyHazardousSubstance: true,
            ReleaseOccurredAtUtc: now,
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateReleaseReporting(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().Contain(v => v.Severity == "Critical" && v.Citation == "40 CFR 302.6(a)");
        result.Value.Violations.Should().Contain(v => v.Severity == "Critical" && v.Citation == "EPCRA Section 304");
        result.Value.RequiredActions.Should().Contain(a => a.Contains("800-424-8802"));
        result.Value.RequiredActions.Should().Contain(a => a.Contains("SERC"));
    }

    [Fact]
    public void EvaluateReleaseReporting_ReleaseBelowRq_ShouldReportAdvisoryOnly()
    {
        var now = UtcTimestamp.Now();

        var input = new CerclaReleaseInput(
            LinkedIncidentManagementId: null,
            SubstanceName: "Sodium Hydroxide",
            CasNumber: "1310-73-2",
            ReleasedQuantity: 10m,
            QuantityUnit: "lbs",
            ReportableQuantityThreshold: 1_000m,
            IsExtremelyHazardousSubstance: false,
            ReleaseOccurredAtUtc: now,
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateReleaseReporting(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeTrue();
        result.Value.Violations.Should().BeEmpty();
        result.Value.Warnings.Should().ContainSingle(w => w.Severity == "Advisory");
    }

    [Fact]
    public void EvaluateReleaseReporting_ReportableReleaseStaleBeyondOneHour_ShouldReportHighStalenessWarning()
    {
        var now = UtcTimestamp.Now();
        var releaseOccurredAt = new UtcTimestamp(now.Value.AddHours(-3));

        var input = new CerclaReleaseInput(
            LinkedIncidentManagementId: null,
            SubstanceName: "Chlorine",
            CasNumber: "7782-50-5",
            ReleasedQuantity: 50m,
            QuantityUnit: "lbs",
            ReportableQuantityThreshold: 10m,
            IsExtremelyHazardousSubstance: true,
            ReleaseOccurredAtUtc: releaseOccurredAt,
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateReleaseReporting(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Warnings.Should().ContainSingle(w => w.Severity == "High");
    }

    [Fact]
    public void EvaluateReleaseReporting_ReleaseExactlyAtRq_ShouldBeReportable()
    {
        var now = UtcTimestamp.Now();

        var input = new CerclaReleaseInput(
            LinkedIncidentManagementId: null,
            SubstanceName: "Sulfuric Acid",
            CasNumber: "7664-93-9",
            ReleasedQuantity: 1_000m,
            QuantityUnit: "lbs",
            ReportableQuantityThreshold: 1_000m,
            IsExtremelyHazardousSubstance: false,
            ReleaseOccurredAtUtc: now,
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateReleaseReporting(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().NotBeEmpty();
    }

    [Fact]
    public void EvaluateReleaseReporting_NegativeQuantity_ShouldFail()
    {
        var now = UtcTimestamp.Now();

        var input = new CerclaReleaseInput(
            LinkedIncidentManagementId: null,
            SubstanceName: "Benzene",
            CasNumber: "71-43-2",
            ReleasedQuantity: -5m,
            QuantityUnit: "lbs",
            ReportableQuantityThreshold: 10m,
            IsExtremelyHazardousSubstance: false,
            ReleaseOccurredAtUtc: now,
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateReleaseReporting(input);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void EvaluateReleaseReporting_EmptySubstanceName_ShouldFail()
    {
        var now = UtcTimestamp.Now();

        var input = new CerclaReleaseInput(
            LinkedIncidentManagementId: null,
            SubstanceName: "   ",
            CasNumber: null,
            ReleasedQuantity: 5m,
            QuantityUnit: "lbs",
            ReportableQuantityThreshold: 10m,
            IsExtremelyHazardousSubstance: false,
            ReleaseOccurredAtUtc: now,
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateReleaseReporting(input);

        result.IsSuccess.Should().BeFalse();
    }
}
