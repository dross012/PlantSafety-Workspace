// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.ControlSystemSafety
//  Layer:          Domain
//  File:           ControlSystemSafetyEngine.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless, deterministic domain service that evaluates ISO 13849-1 /
//                  IEC 62061 safety-related-control-system compliance.
//
//                  Regulatory citations encoded here (verified via research, not fabricated):
//                    - ISO 13849-1 / IEC 62061 define the Performance Level (PL a-e) and, for ISO
//                      13849-1, the architectural Category (B, 1, 2, 3, 4) of a safety-related part
//                      of a control system. PL expresses the average probability of a dangerous
//                      failure per hour; Category B is the basic state-of-the-art category,
//                      Category 1 uses tried-and-tested components/principles, Category 2
//                      introduces checking at intervals, and Categories 3/4 add redundancy and
//                      fault detection (well-known industry knowledge; this engine cites ISO
//                      13849-1 generally for the Category 3/4 architectural specifics rather than
//                      fabricating clause-level detail not confirmed in research).
//                    - This codebase does not yet track a formal PL/Category rating on any real
//                      aggregate (see ControlSystemSafetyInput.cs remarks), so the required PL/
//                      Category is caller-supplied per component from a documented risk
//                      assessment; this engine still consumes real SafetyValvesRelays/
//                      PresenceSensingDevices reliability data (fault counts, reliability/
//                      detection-accuracy scores, inspection currency) as supporting evidence.
//
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Services;
using PlantSafety.Domain.Compliance.Standards.Shared;
using PlantSafety.Domain.Compliance.Standards.Shared.ValueObjects;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Compliance.Standards.ControlSystemSafety.Services;

/// <summary>
/// Evaluates ISO 13849-1 / IEC 62061 safety-related-control-system compliance. Pure/stateless.
/// This codebase does not yet track a formal Performance Level/Category rating on any real
/// aggregate, so the required PL/Category is caller-supplied per component; the engine still
/// consumes REAL SafetyValvesRelays/PresenceSensingDevices reliability data as supporting
/// evidence for the evaluation (fault counts, reliability/detection-accuracy scores, inspection
/// currency) alongside that caller-supplied classification.
/// </summary>
public sealed class ControlSystemSafetyEngine : IDomainService
{
    private const string EngineCode = "ISO_13849";

    /// <summary>
    /// Illustrative internal quality bar (NOT a literal ISO 13849-1/IEC 62061 numeric PFHd
    /// requirement -- real PL/PFHd math requires component-level reliability data this codebase
    /// does not have): reliability/detection-accuracy scores below this percentage are flagged
    /// for review.
    /// </summary>
    private const decimal ReliabilityIllustrativeThreshold = 80m;

    /// <summary>Evaluates ISO 13849-1/IEC 62061 compliance for the supplied site snapshot.</summary>
    public Result<StandardsComplianceReport> EvaluateCompliance(ControlSystemSafetyInput input)
    {
        if (input is null)
            return Result<StandardsComplianceReport>.Failure("Input is required.");

        foreach (var component in input.Components)
        {
            if (component.FaultCount < 0)
                return Result<StandardsComplianceReport>.Failure(
                    $"Component '{component.ComponentTitle}': FaultCount must not be negative.");
        }

        var violations = new List<StandardsComplianceFinding>();
        var warnings = new List<StandardsComplianceFinding>();
        var requiredActions = new List<string>();

        foreach (var component in input.Components)
            EvaluateComponent(component, input.EvaluatedAtUtc, violations, warnings, requiredActions);

        var report = new StandardsComplianceReport(
            EngineCode: EngineCode,
            EvaluatedAtUtc: input.EvaluatedAtUtc,
            Violations: violations,
            Warnings: warnings,
            RequiredActions: requiredActions,
            IsCompliant: violations.Count == 0);

        return Result<StandardsComplianceReport>.Success(report);
    }

    private static void EvaluateComponent(
        SafetyComponentSnapshot component,
        UtcTimestamp evaluatedAtUtc,
        List<StandardsComplianceFinding> violations,
        List<StandardsComplianceFinding> warnings,
        List<string> requiredActions)
    {
        // 1. No formal PL/Category rating.
        if (component.AchievedPerformanceLevel is null || component.Category is null)
        {
            warnings.Add(new StandardsComplianceFinding(
                Citation: "ISO 13849-1",
                Description: $"Component '{component.ComponentTitle}' ({component.EquipmentLocation}) has no formally rated ISO 13849-1 Performance Level/Category -- a risk assessment and PL determination is required.",
                Severity: "High",
                RelatedEntityId: component.ComponentTitle));

            requiredActions.Add(
                $"Commission a documented ISO 13849-1 risk assessment and formal PL/Category rating for component '{component.ComponentTitle}'.");
        }

        // 2. Achieved PL below required PL.
        if (component.AchievedPerformanceLevel.HasValue && component.AchievedPerformanceLevel.Value < component.RequiredPerformanceLevel)
        {
            violations.Add(new StandardsComplianceFinding(
                Citation: "ISO 13849-1",
                Description: $"Component '{component.ComponentTitle}': achieved Performance Level ({component.AchievedPerformanceLevel}) is below the required PL ({component.RequiredPerformanceLevel}) for this safety function per ISO 13849-1.",
                Severity: "Critical",
                RelatedEntityId: component.ComponentTitle));

            requiredActions.Add(
                $"Upgrade or redesign component '{component.ComponentTitle}' to achieve the required Performance Level {component.RequiredPerformanceLevel}.");
        }

        // 3. Past inspection/verification interval.
        if (component.NextInspectionDueAtUtc.HasValue && component.NextInspectionDueAtUtc.Value.Value < evaluatedAtUtc.Value)
        {
            violations.Add(new StandardsComplianceFinding(
                Citation: "ISO 13849-1",
                Description: $"Component '{component.ComponentTitle}' is past its inspection/verification interval (due {component.NextInspectionDueAtUtc}).",
                Severity: "High",
                RelatedEntityId: component.ComponentTitle));

            requiredActions.Add(
                $"Perform the overdue safety-related inspection/verification for component '{component.ComponentTitle}'.");
        }

        // 4. Fault history.
        if (component.FaultCount > 0)
        {
            warnings.Add(new StandardsComplianceFinding(
                Citation: "ISO 13849-1",
                Description: $"Component '{component.ComponentTitle}' has a fault history ({component.FaultCount} recorded fault(s)) that should be reviewed against the diagnostic coverage assumed by its Category ({component.Category}).",
                Severity: "Medium",
                RelatedEntityId: component.ComponentTitle));
        }

        // 5. Reliability/accuracy score below illustrative internal threshold.
        if (component.ReliabilityOrAccuracyScore.HasValue && component.ReliabilityOrAccuracyScore.Value < ReliabilityIllustrativeThreshold)
        {
            warnings.Add(new StandardsComplianceFinding(
                Citation: "Internal quality threshold (illustrative -- not a literal ISO 13849-1/IEC 62061 PFHd figure)",
                Description: $"Component '{component.ComponentTitle}' has a reliability/detection-accuracy score of {component.ReliabilityOrAccuracyScore:N1}%, below the {ReliabilityIllustrativeThreshold:N0}% illustrative operational threshold.",
                Severity: "Medium",
                RelatedEntityId: component.ComponentTitle));
        }
    }
}
