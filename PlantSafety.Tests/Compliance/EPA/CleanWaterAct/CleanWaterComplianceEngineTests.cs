// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.EPA.CleanWaterAct
//  Layer:          Tests (Unit)
//  File:           CleanWaterComplianceEngineTests.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the CleanWaterComplianceEngine domain service.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.EPA.CleanWaterAct.Services;

namespace PlantSafety.Tests.Compliance.EPA.CleanWaterAct;

/// <summary>Unit tests for <see cref="CleanWaterComplianceEngine"/>.</summary>
public sealed class CleanWaterComplianceEngineTests : UnitTestBase
{
    private readonly CleanWaterComplianceEngine _engine = new();

    [Fact]
    public void EvaluateCompliance_ExpiredNpdesPermit_ShouldReportCriticalViolation()
    {
        var input = new CleanWaterComplianceInput(
            Permits:
            [
                new DischargePermitSnapshot(
                    DischargePermitId: Guid.NewGuid(),
                    PermitNumber: "NPDES-001",
                    RegulatoryProgram: "Npdes",
                    EffectiveAtUtc: UtcTimestamp.From(FixedUtcNow.AddYears(-5)),
                    ExpiresAtUtc: UtcTimestamp.From(FixedUtcNow.AddDays(-1)),
                    Status: "Active"),
            ],
            Exceedances: [],
            FacilityStoresOilAboveSpccThreshold: false,
            HasSpccPlan: false,
            SpccPlanLastReviewedAtUtc: null,
            EvaluatedAtUtc: FixedTimestamp);

        var result = _engine.EvaluateCompliance(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().ContainSingle(v => v.Severity == "Critical" && v.Citation.Contains("1311(a)"));
    }

    [Fact]
    public void EvaluateCompliance_RepeatExceedance_ShouldEscalateToCritical()
    {
        var input = new CleanWaterComplianceInput(
            Permits: [],
            Exceedances:
            [
                new LimitExceedanceSnapshot(
                    LimitExceedanceId: Guid.NewGuid(),
                    DischargePointId: Guid.NewGuid(),
                    MeasuredValue: 15m,
                    LimitValue: 10m,
                    ExceedancePercentage: 50m,
                    Severity: "Minor",
                    RepeatExceedanceCount: 3),
            ],
            FacilityStoresOilAboveSpccThreshold: false,
            HasSpccPlan: false,
            SpccPlanLastReviewedAtUtc: null,
            EvaluatedAtUtc: FixedTimestamp);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().ContainSingle(v => v.Severity == "Critical");
    }

    [Fact]
    public void EvaluateCompliance_MissingSpccPlanWhenRequired_ShouldReportCriticalViolation()
    {
        var input = new CleanWaterComplianceInput(
            Permits: [],
            Exceedances: [],
            FacilityStoresOilAboveSpccThreshold: true,
            HasSpccPlan: false,
            SpccPlanLastReviewedAtUtc: null,
            EvaluatedAtUtc: FixedTimestamp);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().ContainSingle(v => v.Severity == "Critical" && v.Citation.Contains("112"));
    }

    [Fact]
    public void EvaluateCompliance_ActivePermitNoExceedancesNoSpccRequirement_ShouldBeCompliant()
    {
        var input = new CleanWaterComplianceInput(
            Permits:
            [
                new DischargePermitSnapshot(
                    DischargePermitId: Guid.NewGuid(),
                    PermitNumber: "NPDES-002",
                    RegulatoryProgram: "Npdes",
                    EffectiveAtUtc: UtcTimestamp.From(FixedUtcNow.AddYears(-1)),
                    ExpiresAtUtc: UtcTimestamp.From(FixedUtcNow.AddYears(3)),
                    Status: "Active"),
            ],
            Exceedances: [],
            FacilityStoresOilAboveSpccThreshold: false,
            HasSpccPlan: false,
            SpccPlanLastReviewedAtUtc: null,
            EvaluatedAtUtc: FixedTimestamp);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.IsCompliant.Should().BeTrue();
        result.Value.Violations.Should().BeEmpty();
    }

    [Fact]
    public void EvaluateCompliance_SpccPlanOverdueForReview_ShouldReportMediumWarning()
    {
        var input = new CleanWaterComplianceInput(
            Permits: [],
            Exceedances: [],
            FacilityStoresOilAboveSpccThreshold: true,
            HasSpccPlan: true,
            SpccPlanLastReviewedAtUtc: UtcTimestamp.From(FixedUtcNow.AddYears(-6)),
            EvaluatedAtUtc: FixedTimestamp);

        var result = _engine.EvaluateCompliance(input);

        result.Value!.IsCompliant.Should().BeTrue();
        result.Value.Warnings.Should().ContainSingle(w => w.Severity == "Medium" && w.Citation.Contains("112.5"));
    }

    [Fact]
    public void EvaluateCompliance_NullInput_ShouldFail()
    {
        var result = _engine.EvaluateCompliance(null!);

        result.IsSuccess.Should().BeFalse();
    }
}
