// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.CleanWaterAct
//  Layer:          Domain
//  File:           CleanWaterComplianceEngine.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless, deterministic domain service that evaluates Clean Water Act
//                  (33 U.S.C. §1251, 40 CFR Parts 122/112) compliance by rolling up
//                  discharge-permit, discharge-limit, and limit-exceedance state already tracked by
//                  PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance, plus SPCC
//                  applicability. There is no persisted aggregate for this engine itself -- it is a
//                  pure calculation over caller-supplied snapshots of the real
//                  DischargePermit/LimitExceedance aggregate state (same "stateless calculation
//                  service" pattern as Compliance.PHMSA.Segregation.Services.SegregationEngine).
//
//                  Regulatory citations encoded here (verified via research, not fabricated):
//                    - It is unlawful to discharge any pollutant from a point source into waters of
//                      the US without an NPDES permit (33 U.S.C. §1311(a), 40 CFR Part 122).
//                    - Effluent limitations are industry-specific (technology-based + water-quality-
//                      based); exceedances are evaluated against the permit's discharge limits
//                      (40 CFR 122).
//                    - SPCC (Spill Prevention, Control, and Countermeasure) plans are required under
//                      40 CFR Part 112 for facilities that could reasonably be expected to discharge
//                      oil to navigable waters/adjoining shorelines above regulatory thresholds.
//                      SPCC plans require review/amendment every 5 years (40 CFR 112.5).
//
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.EPA.Shared;
using PlantSafety.Domain.Compliance.EPA.Shared.ValueObjects;

namespace PlantSafety.Domain.Compliance.EPA.CleanWaterAct.Services;

/// <summary>Input to <see cref="CleanWaterComplianceEngine.EvaluateCompliance"/>.</summary>
public sealed record CleanWaterComplianceInput(
    IReadOnlyList<DischargePermitSnapshot> Permits,
    IReadOnlyList<LimitExceedanceSnapshot> Exceedances,
    bool FacilityStoresOilAboveSpccThreshold,
    bool HasSpccPlan,
    UtcTimestamp? SpccPlanLastReviewedAtUtc,
    UtcTimestamp EvaluatedAtUtc);

/// <summary>
/// Caller-projected snapshot of a real
/// <see cref="PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Aggregates.DischargePermit"/>
/// loaded via its repository interface.
/// </summary>
public sealed record DischargePermitSnapshot(
    Guid DischargePermitId,
    string PermitNumber,
    string RegulatoryProgram,
    UtcTimestamp EffectiveAtUtc,
    UtcTimestamp ExpiresAtUtc,
    string Status);

/// <summary>
/// Caller-projected snapshot of a real
/// <see cref="PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Aggregates.LimitExceedance"/>
/// loaded via its repository interface.
/// </summary>
public sealed record LimitExceedanceSnapshot(
    Guid LimitExceedanceId,
    Guid DischargePointId,
    decimal MeasuredValue,
    decimal LimitValue,
    decimal ExceedancePercentage,
    string Severity,
    int RepeatExceedanceCount);

/// <summary>
/// Evaluates Clean Water Act (33 U.S.C. §1251, 40 CFR Parts 122/112) compliance by rolling up
/// discharge-permit, discharge-limit, and limit-exceedance state already tracked by
/// PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance, plus SPCC applicability.
/// Pure/stateless: callers load the real aggregates via their repository interfaces and project
/// them into the snapshot records this method accepts.
/// </summary>
public sealed class CleanWaterComplianceEngine : IDomainService
{
    private const string ProgramCode = "CWA";

    /// <summary>Window (days) before a discharge permit's expiration at which a renewal warning is raised.</summary>
    private const int PermitRenewalWarningWindowDays = 90;

    /// <summary>Repeat-exceedance count at or above which an exceedance is escalated to Critical regardless of its original severity.</summary>
    private const int ChronicExceedanceRepeatThreshold = 3;

    /// <summary>SPCC plan mandatory review/amendment interval per 40 CFR 112.5.</summary>
    private const int SpccPlanReviewIntervalYears = 5;

    /// <summary>Evaluates Clean Water Act compliance for the supplied permit/exceedance/SPCC snapshot.</summary>
    public Result<EpaComplianceReport> EvaluateCompliance(CleanWaterComplianceInput input)
    {
        if (input is null)
            return Result<EpaComplianceReport>.Failure("Input is required.");
        if (input.Permits is null)
            return Result<EpaComplianceReport>.Failure("Permits list is required.");
        if (input.Exceedances is null)
            return Result<EpaComplianceReport>.Failure("Exceedances list is required.");

        foreach (var permit in input.Permits)
        {
            if (permit is null || string.IsNullOrWhiteSpace(permit.PermitNumber))
                return Result<EpaComplianceReport>.Failure("Each discharge permit record must have a non-empty PermitNumber.");
        }

        var violations = new List<EpaComplianceFinding>();
        var warnings = new List<EpaComplianceFinding>();
        var requiredActions = new List<string>();

        foreach (var permit in input.Permits)
            EvaluatePermitExpiration(permit, input.EvaluatedAtUtc, violations, warnings, requiredActions);

        foreach (var exceedance in input.Exceedances)
            EvaluateExceedance(exceedance, violations, warnings, requiredActions);

        EvaluateSpccApplicability(input, violations, warnings, requiredActions);

        var report = new EpaComplianceReport(
            ProgramCode: ProgramCode,
            EvaluatedAtUtc: input.EvaluatedAtUtc,
            Violations: violations,
            Warnings: warnings,
            RequiredActions: requiredActions,
            IsCompliant: violations.Count == 0);

        return Result<EpaComplianceReport>.Success(report);
    }

    /// <summary>
    /// 33 U.S.C. §1311(a): operating without a valid NPDES permit is a violation once the permit has
    /// expired; a permit expiring within the renewal window is flagged as a warning.
    /// </summary>
    private static void EvaluatePermitExpiration(
        DischargePermitSnapshot permit,
        UtcTimestamp evaluatedAtUtc,
        List<EpaComplianceFinding> violations,
        List<EpaComplianceFinding> warnings,
        List<string> requiredActions)
    {
        var daysUntilExpiration = (permit.ExpiresAtUtc.Value - evaluatedAtUtc.Value).TotalDays;

        if (permit.ExpiresAtUtc.Value < evaluatedAtUtc.Value)
        {
            violations.Add(new EpaComplianceFinding(
                Citation: "33 U.S.C. §1311(a)",
                Description: $"Discharge permit {permit.PermitNumber} has expired -- operating without a valid NPDES permit violates 33 U.S.C. §1311(a).",
                Severity: "Critical",
                RelatedEntityId: permit.DischargePermitId.ToString()));

            requiredActions.Add(
                $"Renew or re-apply for discharge permit {permit.PermitNumber} immediately -- it has expired.");
        }
        else if (daysUntilExpiration <= PermitRenewalWarningWindowDays)
        {
            warnings.Add(new EpaComplianceFinding(
                Citation: "33 U.S.C. §1311(a)",
                Description: $"Discharge permit {permit.PermitNumber} expires {permit.ExpiresAtUtc} (approximately {daysUntilExpiration:F0} day(s) remaining) and should be renewed.",
                Severity: "Medium",
                RelatedEntityId: permit.DischargePermitId.ToString()));

            requiredActions.Add(
                $"Begin the renewal process for discharge permit {permit.PermitNumber} before it expires on {permit.ExpiresAtUtc}.");
        }
    }

    /// <summary>
    /// Clean Water Act NPDES effluent limitation, 40 CFR 122: translates a recorded exceedance's own
    /// severity into a finding, escalating to Critical when the repeat-exceedance count indicates a
    /// chronic pattern.
    /// </summary>
    private static void EvaluateExceedance(
        LimitExceedanceSnapshot exceedance,
        List<EpaComplianceFinding> violations,
        List<EpaComplianceFinding> warnings,
        List<string> requiredActions)
    {
        var isChronic = exceedance.RepeatExceedanceCount >= ChronicExceedanceRepeatThreshold;
        var severity = isChronic ? "Critical" : exceedance.Severity;

        var chronicNote = isChronic
            ? $" This is a repeat/chronic exceedance ({exceedance.RepeatExceedanceCount} occurrences), escalated to Critical."
            : string.Empty;

        var finding = new EpaComplianceFinding(
            Citation: "Clean Water Act NPDES effluent limitation, 40 CFR 122",
            Description: $"Discharge point measured {exceedance.MeasuredValue} against a limit of {exceedance.LimitValue} ({exceedance.ExceedancePercentage:F1}% exceedance).{chronicNote}",
            Severity: severity,
            RelatedEntityId: exceedance.DischargePointId.ToString());

        if (isChronic)
        {
            violations.Add(finding);
            requiredActions.Add(
                $"Investigate and remediate the root cause of the repeat effluent limit exceedance at discharge point {exceedance.DischargePointId} ({exceedance.RepeatExceedanceCount} occurrences).");
        }
        else if (string.Equals(severity, "Critical", StringComparison.OrdinalIgnoreCase)
            || string.Equals(severity, "High", StringComparison.OrdinalIgnoreCase)
            || string.Equals(severity, "Major", StringComparison.OrdinalIgnoreCase))
        {
            violations.Add(finding);
        }
        else
        {
            warnings.Add(finding);
        }
    }

    /// <summary>
    /// 40 CFR Part 112 (SPCC): a facility that could reasonably be expected to discharge oil above
    /// the regulatory threshold must have an SPCC plan, and that plan must be reviewed/amended at
    /// least every 5 years per 40 CFR 112.5.
    /// </summary>
    private static void EvaluateSpccApplicability(
        CleanWaterComplianceInput input,
        List<EpaComplianceFinding> violations,
        List<EpaComplianceFinding> warnings,
        List<string> requiredActions)
    {
        if (!input.FacilityStoresOilAboveSpccThreshold)
            return;

        if (!input.HasSpccPlan)
        {
            violations.Add(new EpaComplianceFinding(
                Citation: "40 CFR Part 112 (SPCC)",
                Description: "Facility stores oil above the SPCC applicability threshold but does not have a Spill Prevention, Control, and Countermeasure (SPCC) plan.",
                Severity: "Critical",
                RelatedEntityId: null));

            requiredActions.Add(
                "Prepare and implement an SPCC plan per 40 CFR Part 112 -- the facility stores oil above the applicability threshold.");
            return;
        }

        if (input.SpccPlanLastReviewedAtUtc is null)
            return;

        var yearsSinceReview = (input.EvaluatedAtUtc.Value - input.SpccPlanLastReviewedAtUtc.Value.Value).TotalDays / 365.25;
        if (yearsSinceReview > SpccPlanReviewIntervalYears)
        {
            warnings.Add(new EpaComplianceFinding(
                Citation: "40 CFR 112.5",
                Description: $"SPCC plan was last reviewed {input.SpccPlanLastReviewedAtUtc} (approximately {yearsSinceReview:F1} years ago) and is due for its mandatory 5-year review/amendment.",
                Severity: "Medium",
                RelatedEntityId: null));

            requiredActions.Add(
                "Review and, if necessary, amend the SPCC plan -- it has not been reviewed within the mandatory 5-year interval (40 CFR 112.5).");
        }
    }
}
