// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.ComplianceRisk
//  Layer:          Abstractions
//  File:           SeverityAwareComplianceScore.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase C.10: wraps an existing ComplianceScore with a
//                  severity-adjusted compliance tier, via SeverityImpactRules.
//                  Never recomputes RequiredElements/CompletedElements/
//                  MissingElements/PercentComplete independently --
//                  BaseComplianceScore remains the single source of truth
//                  for those; this adds only the adjustment.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;

/// <summary>A facility's ComplianceScore, adjusted for a specific safety event's severity.</summary>
/// <param name="FacilityId">The facility this score was computed for.</param>
/// <param name="BaseComplianceScore">The unadjusted compliance score this was derived from.</param>
/// <param name="EventSeverity">The severity of the triggering safety event.</param>
/// <param name="AdjustedComplianceTier">The severity-adjusted compliance tier (see <see cref="SeverityImpactRules.AdjustComplianceTier"/>).</param>
/// <param name="Notes">A short, deterministic, human-readable explanation of the adjustment.</param>
public sealed record SeverityAwareComplianceScore(
    Guid FacilityId,
    ComplianceScore BaseComplianceScore,
    SeverityLevel EventSeverity,
    ComplianceTier AdjustedComplianceTier,
    string Notes);

/// <summary>Builds <see cref="SeverityAwareComplianceScore"/> instances deterministically from an existing <see cref="ComplianceScore"/>.</summary>
public static class SeverityAwareComplianceScoreBuilder
{
    /// <summary>Builds the severity-adjusted compliance score for the given base score and event severity.</summary>
    public static SeverityAwareComplianceScore Build(ComplianceScore baseScore, SeverityLevel eventSeverity)
    {
        ComplianceTier adjusted = SeverityImpactRules.AdjustComplianceTier(baseScore.ComplianceTier, eventSeverity);

        string notes = adjusted == baseScore.ComplianceTier
            ? $"Baseline {baseScore.ComplianceTier} compliance tier unaffected by {eventSeverity} severity event."
            : $"Baseline {baseScore.ComplianceTier} compliance tier capped at {adjusted} due to {eventSeverity} severity event.";

        return new SeverityAwareComplianceScore(baseScore.FacilityId, baseScore, eventSeverity, adjusted, notes);
    }
}
