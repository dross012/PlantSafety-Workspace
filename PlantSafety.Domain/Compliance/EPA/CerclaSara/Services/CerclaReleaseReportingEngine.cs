// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.CerclaSara
//  Layer:          Domain
//  File:           CerclaReleaseReportingEngine.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless, deterministic domain service that evaluates whether a
//                  hazardous-substance release event triggers CERCLA Section 103 / EPCRA
//                  Section 304 emergency release-reporting obligations. There is no persisted
//                  aggregate for this engine -- it is a pure calculation over a caller-supplied
//                  release-event snapshot (same "stateless calculation service" pattern as
//                  Compliance.PHMSA.Segregation.Services.SegregationEngine).
//
//                  Regulatory citations encoded here (verified via research, not fabricated):
//                    - CERCLA reportable quantities (RQs) are listed per-substance in 40 CFR 302
//                      Table 302.4 -- this engine takes the applicable RQ as a caller-supplied
//                      input parameter per release event; it does not hardcode a universal RQ.
//                    - CERCLA release notification, 40 CFR 302.6(a): the person in charge of a
//                      facility must, upon knowledge of a release of a hazardous substance in a
//                      quantity equal to or exceeding its RQ, IMMEDIATELY notify the National
//                      Response Center (800-424-8802).
//                    - EPCRA Section 304 emergency release notification: release of a reportable
//                      quantity of an Extremely Hazardous Substance (EHS, EPCRA Section 302 listed)
//                      or CERCLA hazardous substance within any 24-hour period triggers immediate
//                      notification to the State/Tribal Emergency Response Commission (SERC/TERC)
//                      and Local/Tribal Emergency Planning Committee (LEPC/TEPC) for areas likely
//                      affected.
//
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.EPA.Shared;
using PlantSafety.Domain.Compliance.EPA.Shared.ValueObjects;

namespace PlantSafety.Domain.Compliance.EPA.CerclaSara.Services;

/// <summary>
/// Input to <see cref="CerclaReleaseReportingEngine.EvaluateReleaseReporting"/>.
/// </summary>
/// <param name="LinkedIncidentManagementId">
/// Optional traceability link to the real SafetyEvents.IncidentManagement aggregate this release
/// corresponds to, if any.
/// </param>
/// <param name="SubstanceName">Name/identity of the released substance.</param>
/// <param name="CasNumber">CAS registry number of the substance, if known.</param>
/// <param name="ReleasedQuantity">Quantity released, in <paramref name="QuantityUnit"/>.</param>
/// <param name="QuantityUnit">Unit of measure for the released/threshold quantities (e.g. "lbs", "kg").</param>
/// <param name="ReportableQuantityThreshold">
/// The substance's own CERCLA reportable quantity (RQ) per 40 CFR 302 Table 302.4. Caller-supplied,
/// since the RQ varies per substance and this codebase has no CERCLA substance-RQ lookup table
/// populated yet (that's what EpaThresholdRule reference data, built separately, is for -- the
/// Application-layer handler is expected to look up the RQ there and pass it in).
/// </param>
/// <param name="IsExtremelyHazardousSubstance">
/// EPCRA Section 302 Extremely Hazardous Substance (EHS) list membership, caller-supplied.
/// </param>
public sealed record CerclaReleaseInput(
    Guid? LinkedIncidentManagementId,
    string SubstanceName,
    string? CasNumber,
    decimal ReleasedQuantity,
    string QuantityUnit,
    decimal ReportableQuantityThreshold,
    bool IsExtremelyHazardousSubstance,
    UtcTimestamp ReleaseOccurredAtUtc,
    UtcTimestamp EvaluatedAtUtc);

/// <summary>
/// Evaluates whether a hazardous-substance release event triggers CERCLA Section 103 / EPCRA
/// Section 304 emergency release-reporting obligations. Pure/stateless: the release's substance
/// identity and released quantity must be supplied by the caller (this codebase does not yet
/// track substance/quantity on its incident-management domain model -- see
/// SafetyEvents.IncidentManagement -- so this is accepted as direct engine input rather than
/// sourced from an aggregate).
/// </summary>
public sealed class CerclaReleaseReportingEngine : IDomainService
{
    private const string ProgramCode = "CERCLA_SARA";

    private const string NrcGuidance =
        "Immediately notify the National Response Center (NRC) at 800-424-8802 per 40 CFR 302.6(a).";

    private const string SercLepcGuidance =
        "Immediately notify the State/Tribal Emergency Response Commission (SERC/TERC) and the " +
        "Local/Tribal Emergency Planning Committee (LEPC/TEPC) for areas likely affected, per EPCRA Section 304.";

    /// <summary>
    /// Illustrative operational threshold (NOT a literal legal deadline -- the actual CERCLA
    /// standard is simply "immediately") used to surface a compliance-staleness warning when a
    /// reportable release has gone unaddressed in this engine's evaluation for longer than this
    /// window.
    /// </summary>
    private static readonly TimeSpan ImmediateNotificationStalenessWindow = TimeSpan.FromHours(1);

    /// <summary>Evaluates whether the supplied release event triggers CERCLA/EPCRA reporting obligations.</summary>
    public Result<EpaComplianceReport> EvaluateReleaseReporting(CerclaReleaseInput input)
    {
        if (input is null)
            return Result<EpaComplianceReport>.Failure("Input is required.");
        if (string.IsNullOrWhiteSpace(input.SubstanceName))
            return Result<EpaComplianceReport>.Failure("SubstanceName is required.");
        if (input.ReleasedQuantity < 0m)
            return Result<EpaComplianceReport>.Failure("ReleasedQuantity must not be negative.");
        if (input.ReportableQuantityThreshold < 0m)
            return Result<EpaComplianceReport>.Failure("ReportableQuantityThreshold must not be negative.");

        var violations = new List<EpaComplianceFinding>();
        var warnings = new List<EpaComplianceFinding>();
        var requiredActions = new List<string>();

        var relatedEntityId = input.LinkedIncidentManagementId?.ToString();
        var isReportable = input.ReleasedQuantity >= input.ReportableQuantityThreshold;

        if (isReportable)
        {
            violations.Add(new EpaComplianceFinding(
                Citation: "40 CFR 302.6(a)",
                Description: $"Release of {input.SubstanceName} ({input.ReleasedQuantity} {input.QuantityUnit}) meets or exceeds its CERCLA reportable quantity ({input.ReportableQuantityThreshold} {input.QuantityUnit}) -- immediate notification to the National Response Center (800-424-8802) is required per 40 CFR 302.6(a).",
                Severity: "Critical",
                RelatedEntityId: relatedEntityId));
            requiredActions.Add(NrcGuidance);

            // Conservative default: treat any RQ-triggering release as also EPCRA-reportable,
            // regardless of the caller-supplied IsExtremelyHazardousSubstance flag, since any
            // CERCLA hazardous substance released at/above its RQ within 24 hours triggers
            // EPCRA Section 304 notification too.
            violations.Add(new EpaComplianceFinding(
                Citation: "EPCRA Section 304",
                Description: $"Release of {input.SubstanceName} ({input.ReleasedQuantity} {input.QuantityUnit}) is a reportable quantity release under EPCRA Section 304{(input.IsExtremelyHazardousSubstance ? " (Extremely Hazardous Substance)" : string.Empty)} -- immediate notification to the SERC/TERC and LEPC/TEPC for areas likely affected is required.",
                Severity: "Critical",
                RelatedEntityId: relatedEntityId));
            requiredActions.Add(SercLepcGuidance);

            var elapsed = input.EvaluatedAtUtc.Value - input.ReleaseOccurredAtUtc.Value;
            if (elapsed > ImmediateNotificationStalenessWindow)
            {
                warnings.Add(new EpaComplianceFinding(
                    Citation: "40 CFR 302.6(a) / EPCRA Section 304 ('immediately' standard)",
                    Description: $"Release of {input.SubstanceName} occurred {elapsed.TotalHours:F1} hour(s) ago with no confirmation of notification on record in this engine's input. The legal standard is to notify IMMEDIATELY -- notification urgency increases the longer the release goes unreported.",
                    Severity: "High",
                    RelatedEntityId: relatedEntityId));
            }
        }
        else
        {
            warnings.Add(new EpaComplianceFinding(
                Citation: "40 CFR 302.4 Table 302.4 / EPCRA Section 304",
                Description: $"Release of {input.SubstanceName} ({input.ReleasedQuantity} {input.QuantityUnit}) does not currently meet or exceed its CERCLA/EPCRA reportable quantity ({input.ReportableQuantityThreshold} {input.QuantityUnit}). Recorded for audit-trail completeness -- no reporting obligation triggered by this release.",
                Severity: "Advisory",
                RelatedEntityId: relatedEntityId));
        }

        var report = new EpaComplianceReport(
            ProgramCode: ProgramCode,
            EvaluatedAtUtc: input.EvaluatedAtUtc,
            Violations: violations,
            Warnings: warnings,
            RequiredActions: requiredActions,
            IsCompliant: violations.Count == 0);

        return Result<EpaComplianceReport>.Success(report);
    }
}
