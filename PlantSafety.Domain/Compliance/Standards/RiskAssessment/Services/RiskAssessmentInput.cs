// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.RiskAssessment
//  Layer:          Domain
//  File:           RiskAssessmentInput.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Input records consumed by RiskAssessmentEngine.EvaluateRiskAssessment. This
//                  codebase has no dedicated machinery-risk-assessment aggregate, so callers
//                  (Application-layer handlers, built by a separate later effort) supply the
//                  documented risk assessment's own recorded content directly. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.Standards.RiskAssessment.Enums;

namespace PlantSafety.Domain.Compliance.Standards.RiskAssessment.Services;

/// <summary>
/// Input to <see cref="RiskAssessmentEngine.EvaluateRiskAssessment"/>: the recorded content of a
/// documented machinery risk assessment performed per ISO 12100 / ANSI B11.0 / NSC methodology.
/// </summary>
/// <param name="EquipmentIdentifier">Identifier of the machine/equipment the assessment covers. Must not be empty.</param>
/// <param name="Methodology">The named methodology used, e.g. "ISO 12100", "ANSI B11.0 Annex C", or "NSC".</param>
/// <param name="IdentifiedHazards">The hazards systematically identified during the assessment.</param>
/// <param name="SeverityEstimated">Whether the assessment recorded a severity estimate for identified hazards.</param>
/// <param name="ExposureEstimated">Whether the assessment recorded an exposure (frequency/duration) estimate.</param>
/// <param name="ProbabilityEstimated">Whether the assessment recorded a probability-of-occurrence estimate.</param>
/// <param name="RiskReductionMeasures">The risk-reduction measures documented, each tagged with its hierarchy step.</param>
/// <param name="IsIntegratedSystem">
/// <see langword="true"/> when this equipment is part of a multi-machine integrated system, which
/// per ANSI B11 integrator guidance requires its own system-level risk assessment separate from
/// each individual machine's assessment. This engine cannot verify assessment content depth, so it
/// can only flag this as an advisory reminder.
/// </param>
/// <param name="AssessmentCompletedAtUtc">When the documented risk assessment was completed.</param>
/// <param name="EvaluatedAtUtc">When this compliance evaluation is being performed.</param>
public sealed record RiskAssessmentInput(
    string EquipmentIdentifier,
    string Methodology,
    IReadOnlyList<string> IdentifiedHazards,
    bool SeverityEstimated,
    bool ExposureEstimated,
    bool ProbabilityEstimated,
    IReadOnlyList<RiskReductionMeasureRecord> RiskReductionMeasures,
    bool IsIntegratedSystem,
    UtcTimestamp AssessmentCompletedAtUtc,
    UtcTimestamp EvaluatedAtUtc);

/// <summary>A single documented risk-reduction measure and the ISO 12100 hierarchy step it represents.</summary>
/// <param name="Step">Which of the three ISO 12100 hierarchy steps this measure belongs to.</param>
/// <param name="Description">Free-text description of the measure as recorded in the assessment.</param>
public sealed record RiskReductionMeasureRecord(
    RiskReductionHierarchyStep Step,
    string Description);
