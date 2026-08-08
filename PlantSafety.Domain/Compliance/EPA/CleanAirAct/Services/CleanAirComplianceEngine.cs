// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.CleanAirAct
//  Layer:          Domain
//  File:           CleanAirComplianceEngine.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless, deterministic domain service that evaluates Clean Air Act
//                  (42 U.S.C. §7401, 40 CFR Parts 60/61/63/70) compliance for a plant's air
//                  emission sources. This codebase does not yet have a persisted emission-source
//                  Domain aggregate. Until one exists, callers must supply this data directly
//                  rather than loading it from a repository (same "stateless calculation service"
//                  pattern as Compliance.PHMSA.Segregation.Services.SegregationEngine).
//
//                  Regulatory citations encoded here (verified via research, not fabricated):
//                    - NESHAP (National Emission Standards for Hazardous Air Pollutants, 40 CFR
//                      Parts 61 and 63) regulates hazardous air pollutants (HAPs).
//                    - NSPS (New Source Performance Standards, 40 CFR Part 60) regulates criteria
//                      pollutants from new/modified sources.
//                    - Title V operating permit (40 CFR Part 70) is required for any "major
//                      source" -- emits >= 100 tons/year of any air pollutant, OR >= 10 tons/year
//                      of any single HAP, OR >= 25 tons/year of combined HAPs -- and also for any
//                      source (major or not) subject to an NSPS or NESHAP standard.
//
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.EPA.CleanAirAct.Enums;
using PlantSafety.Domain.Compliance.EPA.Shared;
using PlantSafety.Domain.Compliance.EPA.Shared.ValueObjects;

namespace PlantSafety.Domain.Compliance.EPA.CleanAirAct.Services;

/// <summary>Input to <see cref="CleanAirComplianceEngine.EvaluateCompliance"/>.</summary>
public sealed record CleanAirComplianceInput(
    IReadOnlyList<EmissionSourceRecord> EmissionSources,
    UtcTimestamp EvaluatedAtUtc);

/// <summary>
/// Caller-supplied snapshot of a single air emission source's Clean Air Act regulatory posture.
/// This codebase does not yet have a persisted Domain aggregate for emission sources. Until one
/// exists, callers must supply this data directly rather than loading it from a repository.
/// </summary>
public sealed record EmissionSourceRecord(
    string SourceName,
    decimal AnyPollutantTonsPerYear,
    decimal SingleHapTonsPerYear,
    decimal CombinedHapTonsPerYear,
    bool IsSubjectToNsps,
    bool IsSubjectToNeshap,
    bool HasTitleVPermit,
    UtcTimestamp? TitleVPermitExpiresAtUtc);

/// <summary>
/// Evaluates Clean Air Act (42 U.S.C. §7401, 40 CFR Parts 60/61/63/70) compliance for a plant's air
/// emission sources. Pure/stateless: this codebase does not yet have a persisted emission-source
/// Domain aggregate, so callers must supply emission-source records directly until one exists.
/// </summary>
public sealed class CleanAirComplianceEngine : IDomainService
{
    private const string ProgramCode = "CAA";

    /// <summary>Major-source threshold for any single air pollutant (tons/year), 40 CFR Part 70.</summary>
    private const decimal MajorSourceAnyPollutantThresholdTonsPerYear = 100m;

    /// <summary>Major-source threshold for any single hazardous air pollutant (tons/year), 40 CFR Part 70.</summary>
    private const decimal MajorSourceSingleHapThresholdTonsPerYear = 10m;

    /// <summary>Major-source threshold for combined hazardous air pollutants (tons/year), 40 CFR Part 70.</summary>
    private const decimal MajorSourceCombinedHapThresholdTonsPerYear = 25m;

    /// <summary>Window (days) before a Title V permit's expiration at which a renewal warning is raised.</summary>
    private const int PermitRenewalWarningWindowDays = 180;

    /// <summary>Evaluates Clean Air Act compliance for the supplied emission-source snapshot.</summary>
    public Result<EpaComplianceReport> EvaluateCompliance(CleanAirComplianceInput input)
    {
        if (input is null)
            return Result<EpaComplianceReport>.Failure("Input is required.");
        if (input.EmissionSources is null)
            return Result<EpaComplianceReport>.Failure("EmissionSources list is required.");

        foreach (var source in input.EmissionSources)
        {
            if (source is null || string.IsNullOrWhiteSpace(source.SourceName))
                return Result<EpaComplianceReport>.Failure("Each emission source record must have a non-empty SourceName.");
            if (source.AnyPollutantTonsPerYear < 0m || source.SingleHapTonsPerYear < 0m || source.CombinedHapTonsPerYear < 0m)
                return Result<EpaComplianceReport>.Failure($"Emission source '{source.SourceName}' has a negative tonnage value, which is not physically valid.");
        }

        var violations = new List<EpaComplianceFinding>();
        var warnings = new List<EpaComplianceFinding>();
        var requiredActions = new List<string>();

        foreach (var source in input.EmissionSources)
        {
            var category = ClassifySourceCategory(source);
            EvaluateTitleVPermitRequirement(source, category, violations, requiredActions);
            EvaluateTitleVPermitRenewal(source, input.EvaluatedAtUtc, violations, warnings, requiredActions);
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

    /// <summary>
    /// Classifies a source as <see cref="CaaSourceCategory.MajorSource"/> if it meets or exceeds any
    /// of the three Title V major-source thresholds (40 CFR Part 70); otherwise
    /// <see cref="CaaSourceCategory.NonMajor"/>.
    /// </summary>
    public static CaaSourceCategory ClassifySourceCategory(EmissionSourceRecord source)
    {
        if (source.AnyPollutantTonsPerYear >= MajorSourceAnyPollutantThresholdTonsPerYear
            || source.SingleHapTonsPerYear >= MajorSourceSingleHapThresholdTonsPerYear
            || source.CombinedHapTonsPerYear >= MajorSourceCombinedHapThresholdTonsPerYear)
        {
            return CaaSourceCategory.MajorSource;
        }

        return CaaSourceCategory.NonMajor;
    }

    /// <summary>
    /// Clean Air Act Title V, 40 CFR Part 70: any major source, or any source (major or not)
    /// subject to an NSPS or NESHAP standard, must hold a Title V operating permit.
    /// </summary>
    private static void EvaluateTitleVPermitRequirement(
        EmissionSourceRecord source,
        CaaSourceCategory category,
        List<EpaComplianceFinding> violations,
        List<string> requiredActions)
    {
        var requiresTitleV = category == CaaSourceCategory.MajorSource
            || source.IsSubjectToNsps
            || source.IsSubjectToNeshap;

        if (!requiresTitleV || source.HasTitleVPermit)
            return;

        var applicableStandards = new List<string>();
        if (source.IsSubjectToNeshap)
            applicableStandards.Add("NESHAP, 40 CFR Part 61 or 63");
        if (source.IsSubjectToNsps)
            applicableStandards.Add("NSPS, 40 CFR Part 60");

        var standardsContext = applicableStandards.Count > 0
            ? $" This source is also subject to: {string.Join("; ", applicableStandards)}."
            : string.Empty;

        violations.Add(new EpaComplianceFinding(
            Citation: "Clean Air Act Title V, 40 CFR Part 70",
            Description: $"Emission source '{source.SourceName}' requires a Title V operating permit ({(category == CaaSourceCategory.MajorSource ? "classified as a major source" : "subject to NSPS/NESHAP")}) but does not currently hold one.{standardsContext}",
            Severity: "Critical",
            RelatedEntityId: source.SourceName));

        requiredActions.Add(
            $"Submit a Title V operating permit application for '{source.SourceName}' per 40 CFR Part 70.");
    }

    /// <summary>
    /// Flags an existing Title V permit that has already expired (Critical) or is within the
    /// renewal warning window (High).
    /// </summary>
    private static void EvaluateTitleVPermitRenewal(
        EmissionSourceRecord source,
        UtcTimestamp evaluatedAtUtc,
        List<EpaComplianceFinding> violations,
        List<EpaComplianceFinding> warnings,
        List<string> requiredActions)
    {
        if (!source.HasTitleVPermit || source.TitleVPermitExpiresAtUtc is null)
            return;

        var expiresAtUtc = source.TitleVPermitExpiresAtUtc.Value;
        var daysUntilExpiration = (expiresAtUtc.Value - evaluatedAtUtc.Value).TotalDays;

        if (daysUntilExpiration < 0)
        {
            violations.Add(new EpaComplianceFinding(
                Citation: "Clean Air Act Title V, 40 CFR Part 70",
                Description: $"Emission source '{source.SourceName}' is operating under a Title V permit that expired on {expiresAtUtc}.",
                Severity: "Critical",
                RelatedEntityId: source.SourceName));

            requiredActions.Add(
                $"Renew the expired Title V operating permit for '{source.SourceName}' immediately.");
        }
        else if (daysUntilExpiration <= PermitRenewalWarningWindowDays)
        {
            warnings.Add(new EpaComplianceFinding(
                Citation: "Clean Air Act Title V, 40 CFR Part 70",
                Description: $"Title V operating permit for '{source.SourceName}' expires {expiresAtUtc} (approximately {daysUntilExpiration:F0} day(s) remaining) and should be renewed.",
                Severity: "High",
                RelatedEntityId: source.SourceName));

            requiredActions.Add(
                $"Begin the Title V permit renewal process for '{source.SourceName}' before it expires on {expiresAtUtc}.");
        }
    }
}
