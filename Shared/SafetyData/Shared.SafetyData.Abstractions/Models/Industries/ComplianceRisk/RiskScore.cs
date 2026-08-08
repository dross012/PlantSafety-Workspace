// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.ComplianceRisk
//  Layer:          Abstractions
//  File:           RiskScore.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    A facility's multi-dimensional risk score: four
//                  independently-derived risk levels (hazard, program,
//                  regulator, event) plus a single overall level. Every
//                  dimension is computed from existing data, never invented:
//                    - HazardRiskLevel: the facility industry's hazard-flag
//                      density, reusing the exact same thresholds
//                      Consumers.DashboardMetadataRegistry.RiskTierFor
//                      already uses for RiskTier (<=5 Low, <=7 Medium,
//                      >7 High -- see that file's own documented rationale
//                      for the threshold choice), mapped onto RiskLevel.
//                      Never reaches Critical -- flag density alone, without
//                      a specific triggering event, does not by itself
//                      warrant the platform's highest tier.
//                    - ProgramRiskLevel: the percentage of the facility's
//                      required Program-kind compliance elements that are
//                      still missing, using the exact same thresholds
//                      FacilityComplianceProfileBuilder.Build already uses
//                      for ComplianceRiskLevel (<=10% Low, <=30% Medium,
//                      >30% High), mapped onto RiskLevel. Also never reaches
//                      Critical, for the same reason as HazardRiskLevel.
//                    - RegulatorRiskLevel: the highest-risk RegulatorCategory
//                      among the facility's own Regulators, via
//                      RegulatorCategoryRiskCatalog below (a documented,
//                      per-category judgment -- there is no existing
//                      "regulator risk" concept anywhere upstream to derive
//                      this from mechanically).
//                    - EventRiskLevel: the highest EventSeverity among every
//                      EventCatalog entry whose RelatedPrograms intersect the
//                      facility's own RequiredPrograms, mapped 1:1 onto
//                      RiskLevel (EventSeverity and RiskLevel share the same
//                      four-tier Low/Medium/High/Critical shape by design).
//                      This is the only dimension that can reach Critical,
//                      since EventCatalog is the only place Critical severity
//                      is actually assigned in this codebase.
//                  WEIGHTING RULE for OverallRiskLevel: the maximum (worst)
//                  of the four dimension levels -- not a weighted average.
//                  A weighted average would require inventing numeric
//                  weights this subsystem has no real basis for; "overall
//                  risk is only as good as its weakest/worst dimension" is a
//                  simple, deterministic, defensible aggregation that needs
//                  no invented coefficients.
//                  Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Consumers;
using Shared.SafetyData.Abstractions.Models.Industries.Eventing;
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;

namespace Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;

/// <summary>A risk level, shared across every dimension of <see cref="RiskScore"/> (and mirroring <see cref="EventSeverity"/>'s four tiers exactly).</summary>
public enum RiskLevel
{
    /// <summary>Minimal risk.</summary>
    Low,

    /// <summary>Moderate risk.</summary>
    Medium,

    /// <summary>Elevated risk.</summary>
    High,

    /// <summary>Severe risk.</summary>
    Critical,
}

/// <summary>
/// A single facility's multi-dimensional risk score.
/// </summary>
/// <param name="FacilityId">The facility this score was computed for.</param>
/// <param name="HazardRiskLevel">Risk from the facility industry's hazard-flag density.</param>
/// <param name="ProgramRiskLevel">Risk from the percentage of required programs/program-derived elements still missing.</param>
/// <param name="RegulatorRiskLevel">Risk from the highest-risk regulator category among the facility's regulators.</param>
/// <param name="EventRiskLevel">Risk from the highest severity among event types applicable to the facility's required programs.</param>
/// <param name="OverallRiskLevel">The maximum of the four dimension levels (see this file's WEIGHTING RULE note).</param>
public sealed record RiskScore(
    Guid FacilityId,
    RiskLevel HazardRiskLevel,
    RiskLevel ProgramRiskLevel,
    RiskLevel RegulatorRiskLevel,
    RiskLevel EventRiskLevel,
    RiskLevel OverallRiskLevel);

/// <summary>
/// Total, per-category judgment mapping from <see cref="RegulatorCategory"/> to a
/// <see cref="RiskLevel"/>, used only for <see cref="RiskScore.RegulatorRiskLevel"/>. ATF/
/// DHS_CFATS/MSHA carry inherently higher enforcement/life-safety consequence (explosives,
/// facility security, mining) than the general-duty/administrative categories.
/// </summary>
public static class RegulatorCategoryRiskCatalog
{
    /// <summary>Returns the risk level associated with a <see cref="RegulatorCategory"/>.</summary>
    public static RiskLevel ForCategory(RegulatorCategory category) => category switch
    {
        RegulatorCategory.ATF => RiskLevel.High,
        RegulatorCategory.DHS_CFATS => RiskLevel.High,
        RegulatorCategory.MSHA => RiskLevel.High,
        RegulatorCategory.NFPA => RiskLevel.Medium,
        RegulatorCategory.PHMSA => RiskLevel.Medium,
        RegulatorCategory.USCG => RiskLevel.Medium,
        RegulatorCategory.EPA => RiskLevel.Medium,
        RegulatorCategory.OSHA => RiskLevel.Low,
        RegulatorCategory.FDA => RiskLevel.Low,
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown regulator category."),
    };
}

/// <summary>Builds <see cref="RiskScore"/> instances deterministically from a <see cref="FacilityProfile"/>.</summary>
public static class RiskScoreBuilder
{
    /// <summary>Builds the multi-dimensional risk score for the given facility.</summary>
    public static RiskScore Build(FacilityProfile facility)
    {
        RiskLevel hazardRisk = HazardRiskLevelFor(facility);
        RiskLevel programRisk = ProgramRiskLevelFor(facility);
        RiskLevel regulatorRisk = RegulatorRiskLevelFor(facility);
        RiskLevel eventRisk = EventRiskLevelFor(facility);
        RiskLevel overall = Max(Max(hazardRisk, programRisk), Max(regulatorRisk, eventRisk));

        return new RiskScore(facility.FacilityId, hazardRisk, programRisk, regulatorRisk, eventRisk, overall);
    }

    private static RiskLevel HazardRiskLevelFor(FacilityProfile facility) =>
        SafetyFlagCatalog.Decompose(facility.Flags).Count switch
        {
            <= 5 => RiskLevel.Low,
            <= 7 => RiskLevel.Medium,
            _ => RiskLevel.High,
        };

    private static RiskLevel ProgramRiskLevelFor(FacilityProfile facility)
    {
        int requiredPrograms = facility.ComplianceProfile.RequiredElements.Count(element => element.Kind == ComplianceElementKind.Program);
        if (requiredPrograms == 0)
        {
            return RiskLevel.Low;
        }

        int missingPrograms = facility.ComplianceProfile.MissingElements.Count(element => element.Kind == ComplianceElementKind.Program);
        double missingPercent = (double)missingPrograms / requiredPrograms * 100d;

        return missingPercent switch
        {
            <= 10d => RiskLevel.Low,
            <= 30d => RiskLevel.Medium,
            _ => RiskLevel.High,
        };
    }

    private static RiskLevel RegulatorRiskLevelFor(FacilityProfile facility) =>
        facility.Regulators
            .Select(RegulatorCategoryCatalog.ForRegulator)
            .Select(RegulatorCategoryRiskCatalog.ForCategory)
            .DefaultIfEmpty(RiskLevel.Low)
            .Max();

    private static RiskLevel EventRiskLevelFor(FacilityProfile facility)
    {
        var requiredProgramCodes = facility.RequiredPrograms.Select(program => program.Code).ToHashSet(StringComparer.Ordinal);

        IEnumerable<EventSeverity> applicableSeverities = EventCatalog.All
            .Where(eventDefinition => eventDefinition.RelatedPrograms.Any(program => requiredProgramCodes.Contains(program.Code)))
            .Select(eventDefinition => eventDefinition.Severity);

        EventSeverity? highest = applicableSeverities.Cast<EventSeverity?>().Max();

        return highest switch
        {
            EventSeverity.Critical => RiskLevel.Critical,
            EventSeverity.High => RiskLevel.High,
            EventSeverity.Medium => RiskLevel.Medium,
            EventSeverity.Low => RiskLevel.Low,
            _ => RiskLevel.Low,
        };
    }

    private static RiskLevel Max(RiskLevel first, RiskLevel second) => first >= second ? first : second;
}
