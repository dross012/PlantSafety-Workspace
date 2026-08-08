// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Standards.RiskAssessment
//  Layer:          Tests (Unit)
//  File:           RiskAssessmentEngineTests.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the RiskAssessmentEngine domain service.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.Standards.RiskAssessment.Enums;
using PlantSafety.Domain.Compliance.Standards.RiskAssessment.Services;

namespace PlantSafety.Tests.Compliance.Standards.RiskAssessment;

/// <summary>Unit tests for <see cref="RiskAssessmentEngine"/>.</summary>
public sealed class RiskAssessmentEngineTests : UnitTestBase
{
    private readonly RiskAssessmentEngine _engine = new();

    [Fact]
    public void EvaluateRiskAssessment_NoHazardsIdentified_ShouldReportCriticalViolation()
    {
        var now = UtcTimestamp.Now();

        var input = new RiskAssessmentInput(
            EquipmentIdentifier: "PRESS-001",
            Methodology: "ISO 12100",
            IdentifiedHazards: [],
            SeverityEstimated: true,
            ExposureEstimated: true,
            ProbabilityEstimated: true,
            RiskReductionMeasures: [new RiskReductionMeasureRecord(RiskReductionHierarchyStep.TechnicalProtectiveMeasures, "Light curtain installed")],
            IsIntegratedSystem: false,
            AssessmentCompletedAtUtc: now,
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateRiskAssessment(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().ContainSingle(v => v.Severity == "Critical" && v.Description.Contains("No hazards identified"));
    }

    [Fact]
    public void EvaluateRiskAssessment_MissingRiskEstimation_ShouldReportHighViolation()
    {
        var now = UtcTimestamp.Now();

        var input = new RiskAssessmentInput(
            EquipmentIdentifier: "PRESS-002",
            Methodology: "ANSI B11.0 Annex C",
            IdentifiedHazards: ["Pinch point"],
            SeverityEstimated: true,
            ExposureEstimated: false,
            ProbabilityEstimated: true,
            RiskReductionMeasures: [new RiskReductionMeasureRecord(RiskReductionHierarchyStep.InherentlySafeDesign, "Redesigned to remove pinch point")],
            IsIntegratedSystem: false,
            AssessmentCompletedAtUtc: now,
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateRiskAssessment(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Violations.Should().ContainSingle(v => v.Severity == "High" && v.Description.Contains("severity/exposure/probability"));
    }

    [Fact]
    public void EvaluateRiskAssessment_NoRiskReductionMeasures_ShouldReportCriticalViolation()
    {
        var now = UtcTimestamp.Now();

        var input = new RiskAssessmentInput(
            EquipmentIdentifier: "PRESS-003",
            Methodology: "ISO 12100",
            IdentifiedHazards: ["Crush hazard"],
            SeverityEstimated: true,
            ExposureEstimated: true,
            ProbabilityEstimated: true,
            RiskReductionMeasures: [],
            IsIntegratedSystem: false,
            AssessmentCompletedAtUtc: now,
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateRiskAssessment(input);

        result.Value!.Violations.Should().ContainSingle(v => v.Severity == "Critical" && v.Description.Contains("No risk reduction measures"));
    }

    [Fact]
    public void EvaluateRiskAssessment_RelianceSolelyOnInformationForUse_ShouldReportHighWarning()
    {
        var now = UtcTimestamp.Now();

        var input = new RiskAssessmentInput(
            EquipmentIdentifier: "PRESS-004",
            Methodology: "ISO 12100",
            IdentifiedHazards: ["Crush hazard"],
            SeverityEstimated: true,
            ExposureEstimated: true,
            ProbabilityEstimated: true,
            RiskReductionMeasures: [new RiskReductionMeasureRecord(RiskReductionHierarchyStep.InformationForUse, "Warning label affixed")],
            IsIntegratedSystem: false,
            AssessmentCompletedAtUtc: now,
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateRiskAssessment(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Violations.Should().BeEmpty();
        result.Value.Warnings.Should().ContainSingle(w => w.Severity == "High" && w.Description.Contains("solely on information-for-use"));
    }

    [Fact]
    public void EvaluateRiskAssessment_IntegratedSystem_ShouldReportAdvisoryFinding()
    {
        var now = UtcTimestamp.Now();

        var input = new RiskAssessmentInput(
            EquipmentIdentifier: "LINE-005",
            Methodology: "ANSI B11.0 Annex C",
            IdentifiedHazards: ["Entanglement"],
            SeverityEstimated: true,
            ExposureEstimated: true,
            ProbabilityEstimated: true,
            RiskReductionMeasures: [new RiskReductionMeasureRecord(RiskReductionHierarchyStep.TechnicalProtectiveMeasures, "Interlocked guard")],
            IsIntegratedSystem: true,
            AssessmentCompletedAtUtc: now,
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateRiskAssessment(input);

        result.Value!.Warnings.Should().ContainSingle(w => w.Severity == "Advisory" && w.Description.Contains("system-level risk assessment"));
    }

    [Fact]
    public void EvaluateRiskAssessment_OlderThanFiveYears_ShouldReportMediumWarning()
    {
        var now = UtcTimestamp.Now();
        var completedAt = new UtcTimestamp(now.Value.AddYears(-6));

        var input = new RiskAssessmentInput(
            EquipmentIdentifier: "PRESS-006",
            Methodology: "ISO 12100",
            IdentifiedHazards: ["Crush hazard"],
            SeverityEstimated: true,
            ExposureEstimated: true,
            ProbabilityEstimated: true,
            RiskReductionMeasures: [new RiskReductionMeasureRecord(RiskReductionHierarchyStep.TechnicalProtectiveMeasures, "Guard installed")],
            IsIntegratedSystem: false,
            AssessmentCompletedAtUtc: completedAt,
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateRiskAssessment(input);

        result.Value!.Warnings.Should().ContainSingle(w => w.Severity == "Medium" && w.Description.Contains("over 5 years old"));
    }

    [Fact]
    public void EvaluateRiskAssessment_FullyCompliantAssessment_ShouldReportNoFindings()
    {
        var now = UtcTimestamp.Now();
        var completedAt = new UtcTimestamp(now.Value.AddMonths(-3));

        var input = new RiskAssessmentInput(
            EquipmentIdentifier: "PRESS-007",
            Methodology: "ISO 12100",
            IdentifiedHazards: ["Crush hazard", "Entanglement hazard"],
            SeverityEstimated: true,
            ExposureEstimated: true,
            ProbabilityEstimated: true,
            RiskReductionMeasures:
            [
                new RiskReductionMeasureRecord(RiskReductionHierarchyStep.InherentlySafeDesign, "Removed pinch point via redesign"),
                new RiskReductionMeasureRecord(RiskReductionHierarchyStep.TechnicalProtectiveMeasures, "Interlocked guard installed"),
                new RiskReductionMeasureRecord(RiskReductionHierarchyStep.InformationForUse, "Operator warning label affixed"),
            ],
            IsIntegratedSystem: false,
            AssessmentCompletedAtUtc: completedAt,
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateRiskAssessment(input);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompliant.Should().BeTrue();
        result.Value.Violations.Should().BeEmpty();
        result.Value.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void EvaluateRiskAssessment_EmptyEquipmentIdentifier_ShouldFail()
    {
        var now = UtcTimestamp.Now();

        var input = new RiskAssessmentInput(
            EquipmentIdentifier: string.Empty,
            Methodology: "ISO 12100",
            IdentifiedHazards: ["Crush hazard"],
            SeverityEstimated: true,
            ExposureEstimated: true,
            ProbabilityEstimated: true,
            RiskReductionMeasures: [new RiskReductionMeasureRecord(RiskReductionHierarchyStep.TechnicalProtectiveMeasures, "Guard installed")],
            IsIntegratedSystem: false,
            AssessmentCompletedAtUtc: now,
            EvaluatedAtUtc: now);

        var result = _engine.EvaluateRiskAssessment(input);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }
}
