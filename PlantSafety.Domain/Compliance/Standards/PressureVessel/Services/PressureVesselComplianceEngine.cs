// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.PressureVessel
//  Layer:          Domain
//  File:           PressureVesselComplianceEngine.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless, deterministic domain service that evaluates ASME BPVC (Boiler
//                  &amp; Pressure Vessel Code) compliance. There is no persisted pressure-vessel
//                  domain aggregate anywhere in this codebase -- this is a pure calculation over a
//                  caller-supplied snapshot of pressure-vessel inventory/inspection data (same
//                  "stateless calculation service" pattern as
//                  Compliance.EPA.Rcra.Services.RcraWasteComplianceEngine).
//
//                  Regulatory facts encoded here (verified via research, not fabricated):
//                    - ASME BPVC: 11 sections/15 subsections; jurisdictionally adopted by every US
//                      state/Canadian province; new edition every 2 years.
//                    - Inspection frequency is NOT uniform -- it varies by equipment type, operating
//                      pressure, and jurisdiction. No single universal interval exists, so this
//                      engine evaluates against a caller-supplied, jurisdiction/equipment-specific
//                      inspection due date rather than a hardcoded interval.
//                    - Most jurisdictions require approximately 30 days' prior notice for new or
//                      replacement pressure-vessel installations. This 30-day figure is the
//                      commonly-cited illustrative figure from research, NOT a universal legal
//                      requirement -- the actual notice period varies by jurisdiction and must be
//                      confirmed with the applicable state/local authority.
//                    - Insurance-company inspectors typically need 2-4 weeks to schedule an
//                      inspection.
//
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Services;
using PlantSafety.Domain.Compliance.Standards.Shared;
using PlantSafety.Domain.Compliance.Standards.Shared.ValueObjects;

namespace PlantSafety.Domain.Compliance.Standards.PressureVessel.Services;

/// <summary>
/// Evaluates ASME BPVC (Boiler &amp; Pressure Vessel Code) compliance. Pure/stateless. This codebase
/// has NO pressure-vessel domain aggregate anywhere -- inspection intervals are not standardized
/// by ASME BPVC itself (they vary by equipment type, operating pressure, and jurisdiction), so
/// this engine evaluates against a caller-supplied, jurisdiction/equipment-specific inspection
/// due date rather than a hardcoded interval.
/// </summary>
public sealed class PressureVesselComplianceEngine : IDomainService
{
    private const string EngineCode = "ASME-BPVC-PressureVessel";

    /// <summary>
    /// Illustrative, commonly-cited jurisdictional prior-notice window (days) for new/replacement
    /// pressure-vessel installations -- NOT a universal legal requirement; actual requirement varies
    /// by jurisdiction.
    /// </summary>
    private const int IllustrativeInstallationNoticeWindowDays = 30;

    /// <summary>Window (days) before an inspection due date at which this engine issues an advance-scheduling reminder.</summary>
    private const int ApproachingInspectionWindowDays = 30;

    /// <summary>Evaluates ASME BPVC compliance for the supplied pressure-vessel snapshot.</summary>
    public Result<StandardsComplianceReport> EvaluateCompliance(PressureVesselComplianceInput input)
    {
        if (input is null)
            return Result<StandardsComplianceReport>.Failure("Input is required.");

        foreach (var vessel in input.Vessels)
        {
            if (string.IsNullOrWhiteSpace(vessel.VesselIdentifier))
                return Result<StandardsComplianceReport>.Failure("VesselIdentifier is required for every vessel.");
            if (vessel.MaxAllowableWorkingPressurePsi is < 0m)
                return Result<StandardsComplianceReport>.Failure($"MaxAllowableWorkingPressurePsi must not be negative (vessel '{vessel.VesselIdentifier}').");
        }

        var violations = new List<StandardsComplianceFinding>();
        var warnings = new List<StandardsComplianceFinding>();
        var requiredActions = new List<string>();

        foreach (var vessel in input.Vessels)
        {
            EvaluateInspectionDueDate(vessel, input.EvaluatedAtUtc, violations, warnings, requiredActions);
            EvaluateJurisdictionalCertificate(vessel, violations, requiredActions);
            EvaluateInstallationNotice(vessel, input.EvaluatedAtUtc, violations, requiredActions);
        }

        var report = new StandardsComplianceReport(
            EngineCode: EngineCode,
            EvaluatedAtUtc: input.EvaluatedAtUtc,
            Violations: violations,
            Warnings: warnings,
            RequiredActions: requiredActions,
            IsCompliant: violations.Count == 0);

        return Result<StandardsComplianceReport>.Success(report);
    }

    private static void EvaluateInspectionDueDate(
        PressureVesselRecord vessel,
        UtcTimestamp evaluatedAtUtc,
        List<StandardsComplianceFinding> violations,
        List<StandardsComplianceFinding> warnings,
        List<string> requiredActions)
    {
        if (!vessel.NextInspectionDueAtUtc.HasValue)
            return;

        var dueAtUtc = vessel.NextInspectionDueAtUtc.Value;

        if (dueAtUtc.Value < evaluatedAtUtc.Value)
        {
            violations.Add(new StandardsComplianceFinding(
                Citation: "ASME BPVC / jurisdictional pressure-vessel inspection requirement",
                Description: $"Vessel '{vessel.VesselIdentifier}' ({vessel.VesselType}) is past its jurisdiction-specific inspection due date ({dueAtUtc}).",
                Severity: "Critical",
                RelatedEntityId: vessel.VesselIdentifier));

            requiredActions.Add($"Schedule an inspection for vessel '{vessel.VesselIdentifier}' immediately -- it is past its inspection due date of {dueAtUtc}.");
            return;
        }

        var daysUntilDue = (dueAtUtc.Value - evaluatedAtUtc.Value).TotalDays;
        if (daysUntilDue <= ApproachingInspectionWindowDays)
        {
            warnings.Add(new StandardsComplianceFinding(
                Citation: "ASME BPVC / jurisdictional pressure-vessel inspection requirement",
                Description: $"Vessel '{vessel.VesselIdentifier}' ({vessel.VesselType}) has an inspection due {dueAtUtc} (approximately {Math.Max(0, daysUntilDue):F0} day(s) away). Insurance-company inspectors typically need 2-4 weeks to schedule.",
                Severity: "Medium",
                RelatedEntityId: vessel.VesselIdentifier));

            requiredActions.Add($"Begin scheduling the upcoming inspection for vessel '{vessel.VesselIdentifier}' (due {dueAtUtc}) -- insurance-company inspectors typically need 2-4 weeks lead time.");
        }
    }

    private static void EvaluateJurisdictionalCertificate(
        PressureVesselRecord vessel, List<StandardsComplianceFinding> violations, List<string> requiredActions)
    {
        if (vessel.HasCurrentJurisdictionalCertificate)
            return;

        violations.Add(new StandardsComplianceFinding(
            Citation: "ASME BPVC jurisdictional operating certificate/permit requirement",
            Description: $"Vessel '{vessel.VesselIdentifier}' ({vessel.VesselType}) does not have a current jurisdictional operating certificate/permit.",
            Severity: "Critical",
            RelatedEntityId: vessel.VesselIdentifier));

        requiredActions.Add($"Obtain a current jurisdictional operating certificate/permit for vessel '{vessel.VesselIdentifier}' before continued operation.");
    }

    private static void EvaluateInstallationNotice(
        PressureVesselRecord vessel, UtcTimestamp evaluatedAtUtc, List<StandardsComplianceFinding> violations, List<string> requiredActions)
    {
        if (!vessel.NewOrReplacementInstallationPending)
            return;

        var noticeFiledTooRecently = vessel.InstallationNoticeFiledAtUtc is null
            || (evaluatedAtUtc.Value - vessel.InstallationNoticeFiledAtUtc.Value.Value).TotalDays < IllustrativeInstallationNoticeWindowDays;

        if (!noticeFiledTooRecently)
            return;

        violations.Add(new StandardsComplianceFinding(
            Citation: "Illustrative jurisdictional prior-notice practice (confirm exact requirement with the applicable state/local jurisdiction)",
            Description: $"Vessel '{vessel.VesselIdentifier}' ({vessel.VesselType}) has a new/replacement installation pending. Most jurisdictions require approximately {IllustrativeInstallationNoticeWindowDays} days' prior notice for new or replacement pressure-vessel installations; " +
                (vessel.InstallationNoticeFiledAtUtc is null
                    ? "no installation notice has been filed."
                    : $"the notice filed on {vessel.InstallationNoticeFiledAtUtc} does not appear to satisfy that window."),
            Severity: "High",
            RelatedEntityId: vessel.VesselIdentifier));

        requiredActions.Add($"File jurisdictional prior notice for the pending installation of vessel '{vessel.VesselIdentifier}' -- confirm the exact required notice period with the applicable state/local jurisdiction.");
    }
}
