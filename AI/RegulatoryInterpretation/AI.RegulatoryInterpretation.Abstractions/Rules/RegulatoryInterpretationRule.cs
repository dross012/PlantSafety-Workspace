// ============================================================================
//  Project:        AI.RegulatoryInterpretation
//  Module:         Abstractions.Rules
//  File:           RegulatoryInterpretationRule.cs
//  Created:        2026-08-08
//  Description:    Phase E.4: a single jurisdiction's deterministic
//                  interpretation -- human-readable guidance plus its
//                  reporting window (reusing Shared.FacilityDomain's
//                  Phase C.4 RegulatoryMetadata.TypicalReportingWindowHours,
//                  not re-deriving it). RegulatoryInterpretationCatalog's
//                  guidance text is a documented editorial judgment
//                  (mirrors Shared.SafetyData's RegulatorCategoryRiskCatalog
//                  precedent), not a mechanically derived fact.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Regulatory;

namespace AI.RegulatoryInterpretation.Abstractions.Rules;

/// <summary>A single jurisdiction's deterministic interpretation: guidance text plus its reporting window.</summary>
public sealed record RegulatoryInterpretationRule(
    RegulatoryJurisdiction Jurisdiction,
    string Guidance,
    int ReportingWindowHours);

/// <summary>A deterministic catalog of interpretation guidance for every <see cref="RegulatoryJurisdiction"/>.</summary>
public static class RegulatoryInterpretationCatalog
{
    /// <summary>Returns the interpretation rule for the given jurisdiction.</summary>
    public static RegulatoryInterpretationRule InterpretationFor(RegulatoryJurisdiction jurisdiction) => jurisdiction switch
    {
        RegulatoryJurisdiction.OSHA => new(jurisdiction, "Report to OSHA within the required window; document root cause, injuries, and corrective actions.", RegulatoryMetadata.TypicalReportingWindowHours(jurisdiction)),
        RegulatoryJurisdiction.EPA => new(jurisdiction, "Assess environmental release quantity against reportable thresholds; notify EPA if exceeded.", RegulatoryMetadata.TypicalReportingWindowHours(jurisdiction)),
        RegulatoryJurisdiction.DOT => new(jurisdiction, "Determine if the incident involves a DOT-regulated hazardous materials shipment; file an incident report if so.", RegulatoryMetadata.TypicalReportingWindowHours(jurisdiction)),
        RegulatoryJurisdiction.NFPA => new(jurisdiction, "Review fire/life-safety code compliance for the affected area; document any code deviations found.", RegulatoryMetadata.TypicalReportingWindowHours(jurisdiction)),
        RegulatoryJurisdiction.State => new(jurisdiction, "Check state-specific reporting requirements, which may be stricter than federal minimums.", RegulatoryMetadata.TypicalReportingWindowHours(jurisdiction)),
        RegulatoryJurisdiction.Local => new(jurisdiction, "Notify local emergency services/authorities having jurisdiction as required by local ordinance.", RegulatoryMetadata.TypicalReportingWindowHours(jurisdiction)),
        _ => throw new ArgumentOutOfRangeException(nameof(jurisdiction), jurisdiction, "Unknown RegulatoryJurisdiction."),
    };
}
