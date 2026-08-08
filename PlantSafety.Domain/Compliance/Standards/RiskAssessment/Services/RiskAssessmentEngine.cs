// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.RiskAssessment
//  Layer:          Domain
//  File:           RiskAssessmentEngine.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless, deterministic domain service that evaluates whether a documented
//                  machinery risk assessment meets the minimum structural requirements of ISO 12100
//                  (Safety of machinery -- General principles for design -- Risk assessment and risk
//                  reduction) and ANSI B11.0-2023 (Safety of Machinery -- General Requirements and
//                  Risk Assessment). There is no persisted risk-assessment aggregate in this codebase
//                  -- this is a pure calculation over a caller-supplied snapshot of the assessment's
//                  own recorded content (same "stateless calculation service" pattern as
//                  Compliance.EPA.Rcra.Services.RcraWasteComplianceEngine and
//                  Compliance.PHMSA.Segregation.Services.SegregationEngine).
//
//                  Regulatory/standards facts encoded here (verified via research, not fabricated):
//                    - ISO 12100 risk-reduction hierarchy: (1) inherently safe design, (2) technical
//                      protective measures (guards/interlocks/light curtains/safety functions),
//                      (3) information for use (warnings/manuals/instructions for residual risk).
//                      Risk assessment requires systematic hazard identification and risk estimation
//                      (severity x exposure x probability) across the machine's full lifecycle
//                      (commissioning through decommissioning).
//                    - ANSI B11.0-2023: harmonized/equivalent risk-assessment methodology; documented
//                      risk assessment required using B11.0 Annex C, ISO 12100, or the NSC method,
//                      considering severity/exposure/probability. Integrated systems (multiple
//                      machines combined) need their own system-level risk assessment separate from
//                      each individual machine's assessment (ANSI B11 integrator guidance).
//                    - No universal periodic re-assessment interval was found in research for
//                      machinery risk assessments; the 5-year interval used below is an illustrative,
//                      commonly-cited industry-practice figure -- NOT a literal regulatory deadline --
//                      clearly documented as such at the point of use.
//
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Services;
using PlantSafety.Domain.Compliance.Standards.RiskAssessment.Enums;
using PlantSafety.Domain.Compliance.Standards.Shared;
using PlantSafety.Domain.Compliance.Standards.Shared.ValueObjects;

namespace PlantSafety.Domain.Compliance.Standards.RiskAssessment.Services;

/// <summary>
/// Evaluates whether a documented machinery risk assessment (ISO 12100 / ANSI B11.0 methodology)
/// meets the minimum structural requirements: hazard identification, severity/exposure/probability
/// estimation, and risk reduction following the 3-step hierarchy. Pure/stateless -- callers supply
/// the risk assessment's own recorded content as input.
/// </summary>
public sealed class RiskAssessmentEngine : IDomainService
{
    private const string EngineCode = "ISO12100-ANSIB11.0-RiskAssessment";

    /// <summary>
    /// Illustrative periodic-review interval (years), NOT a literal universal regulatory deadline --
    /// no single re-assessment interval was found in research; 5 years is a commonly-cited industry
    /// practice figure used here only as a recommendation trigger.
    /// </summary>
    private const int IllustrativePeriodicReviewIntervalYears = 5;

    /// <summary>Evaluates the supplied documented risk assessment for structural ISO 12100 / ANSI B11.0 compliance.</summary>
    public Result<StandardsComplianceReport> EvaluateRiskAssessment(RiskAssessmentInput input)
    {
        if (input is null)
            return Result<StandardsComplianceReport>.Failure("Input is required.");
        if (string.IsNullOrWhiteSpace(input.EquipmentIdentifier))
            return Result<StandardsComplianceReport>.Failure("EquipmentIdentifier is required.");

        var violations = new List<StandardsComplianceFinding>();
        var warnings = new List<StandardsComplianceFinding>();
        var requiredActions = new List<string>();

        EvaluateHazardIdentification(input, violations, requiredActions);
        EvaluateRiskEstimation(input, violations, requiredActions);
        EvaluateRiskReductionMeasures(input, violations, warnings, requiredActions);
        EvaluateIntegratedSystem(input, warnings);
        EvaluatePeriodicReview(input, warnings, requiredActions);

        var report = new StandardsComplianceReport(
            EngineCode: EngineCode,
            EvaluatedAtUtc: input.EvaluatedAtUtc,
            Violations: violations,
            Warnings: warnings,
            RequiredActions: requiredActions,
            IsCompliant: violations.Count == 0);

        return Result<StandardsComplianceReport>.Success(report);
    }

    private static void EvaluateHazardIdentification(
        RiskAssessmentInput input, List<StandardsComplianceFinding> violations, List<string> requiredActions)
    {
        if (input.IdentifiedHazards.Count != 0)
            return;

        violations.Add(new StandardsComplianceFinding(
            Citation: "ISO 12100 / ANSI B11.0",
            Description: "No hazards identified -- a documented risk assessment per ISO 12100 / ANSI B11.0 requires systematic hazard identification.",
            Severity: "Critical",
            RelatedEntityId: input.EquipmentIdentifier));

        requiredActions.Add(
            $"Perform systematic hazard identification for equipment {input.EquipmentIdentifier} per ISO 12100 / ANSI B11.0 before this risk assessment can be considered complete.");
    }

    private static void EvaluateRiskEstimation(
        RiskAssessmentInput input, List<StandardsComplianceFinding> violations, List<string> requiredActions)
    {
        if (input.SeverityEstimated && input.ExposureEstimated && input.ProbabilityEstimated)
            return;

        violations.Add(new StandardsComplianceFinding(
            Citation: "ISO 12100 / ANSI B11.0",
            Description: "Risk assessment is missing one or more of severity/exposure/probability estimation, required by ISO 12100 / ANSI B11.0.",
            Severity: "High",
            RelatedEntityId: input.EquipmentIdentifier));

        requiredActions.Add(
            $"Complete severity, exposure, and probability estimation for equipment {input.EquipmentIdentifier}'s risk assessment per ISO 12100 / ANSI B11.0.");
    }

    private static void EvaluateRiskReductionMeasures(
        RiskAssessmentInput input,
        List<StandardsComplianceFinding> violations,
        List<StandardsComplianceFinding> warnings,
        List<string> requiredActions)
    {
        if (input.RiskReductionMeasures.Count == 0)
        {
            violations.Add(new StandardsComplianceFinding(
                Citation: "ISO 12100 / ANSI B11.0",
                Description: "No risk reduction measures documented.",
                Severity: "Critical",
                RelatedEntityId: input.EquipmentIdentifier));

            requiredActions.Add(
                $"Document risk reduction measures for equipment {input.EquipmentIdentifier} following the ISO 12100 hierarchy (inherently safe design, technical protective measures, information for use).");
            return;
        }

        var hasDesignOrTechnicalMeasure = input.RiskReductionMeasures.Any(m =>
            m.Step is RiskReductionHierarchyStep.InherentlySafeDesign or RiskReductionHierarchyStep.TechnicalProtectiveMeasures);

        if (!hasDesignOrTechnicalMeasure)
        {
            warnings.Add(new StandardsComplianceFinding(
                Citation: "ISO 12100",
                Description: "Risk reduction relies solely on information-for-use (warnings/instructions) -- ISO 12100's hierarchy expects inherently-safe-design and technical-protective-measures to be exhausted first.",
                Severity: "High",
                RelatedEntityId: input.EquipmentIdentifier));

            requiredActions.Add(
                $"Evaluate inherently-safe-design and technical-protective-measure options for equipment {input.EquipmentIdentifier} before relying solely on warnings/instructions.");
        }
    }

    private static void EvaluateIntegratedSystem(RiskAssessmentInput input, List<StandardsComplianceFinding> warnings)
    {
        if (!input.IsIntegratedSystem)
            return;

        // This engine cannot verify content depth -- it can only flag as an advisory reminder.
        warnings.Add(new StandardsComplianceFinding(
            Citation: "ANSI B11 integrator guidance",
            Description: "Integrated/multi-machine systems require their own system-level risk assessment separate from individual machine assessments (ANSI B11 integrator guidance).",
            Severity: "Advisory",
            RelatedEntityId: input.EquipmentIdentifier));
    }

    private static void EvaluatePeriodicReview(
        RiskAssessmentInput input, List<StandardsComplianceFinding> warnings, List<string> requiredActions)
    {
        var ageInDays = (input.EvaluatedAtUtc.Value - input.AssessmentCompletedAtUtc.Value).TotalDays;
        var illustrativeThresholdDays = IllustrativePeriodicReviewIntervalYears * 365.25;

        if (ageInDays <= illustrativeThresholdDays)
            return;

        warnings.Add(new StandardsComplianceFinding(
            Citation: "Illustrative industry practice (no universal ISO 12100 / ANSI B11.0 re-assessment interval found)",
            Description: $"Risk assessment is over {IllustrativePeriodicReviewIntervalYears} years old (completed {input.AssessmentCompletedAtUtc}) -- periodic review is recommended as equipment/process conditions change.",
            Severity: "Medium",
            RelatedEntityId: input.EquipmentIdentifier));

        requiredActions.Add(
            $"Schedule a periodic review of the risk assessment for equipment {input.EquipmentIdentifier} -- it is over {IllustrativePeriodicReviewIntervalYears} years old.");
    }
}
