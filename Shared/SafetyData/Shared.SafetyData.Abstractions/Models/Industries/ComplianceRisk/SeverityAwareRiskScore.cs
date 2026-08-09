// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.ComplianceRisk
//  Layer:          Abstractions
//  File:           SeverityAwareRiskScore.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase C.10: wraps an existing RiskScore with a
//                  severity/injury/failure/chain-adjusted overall risk
//                  level, via SeverityImpactRules. Never recomputes the
//                  four base dimensions independently -- BaseRiskScore
//                  remains the single source of truth for those; this adds
//                  only the adjustment.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;

/// <summary>A facility's RiskScore, adjusted for a specific safety event's severity/injury/failure/chain context.</summary>
/// <param name="FacilityId">The facility this score was computed for.</param>
/// <param name="BaseRiskScore">The unadjusted, dimension-by-dimension risk score this was derived from.</param>
/// <param name="EventSeverity">The severity of the triggering safety event.</param>
/// <param name="EmployeeInjuryCount">The cumulative employee injury count contributing to this adjustment.</param>
/// <param name="MachineFailureCount">The cumulative machine failure count contributing to this adjustment.</param>
/// <param name="EventChainLength">The length of the multi-hazard event chain contributing to this adjustment.</param>
/// <param name="AdjustedOverallRiskLevel">The severity-adjusted overall risk level (see <see cref="SeverityImpactRules.AdjustRiskLevel"/>).</param>
/// <param name="Notes">A short, deterministic, human-readable explanation of the adjustment.</param>
public sealed record SeverityAwareRiskScore(
    Guid FacilityId,
    RiskScore BaseRiskScore,
    SeverityLevel EventSeverity,
    int EmployeeInjuryCount,
    int MachineFailureCount,
    int EventChainLength,
    RiskLevel AdjustedOverallRiskLevel,
    string Notes);

/// <summary>Builds <see cref="SeverityAwareRiskScore"/> instances deterministically from an existing <see cref="RiskScore"/>.</summary>
public static class SeverityAwareRiskScoreBuilder
{
    /// <summary>Builds the severity-adjusted risk score for the given base score and event context.</summary>
    public static SeverityAwareRiskScore Build(RiskScore baseScore, SeverityLevel eventSeverity, int employeeInjuryCount, int machineFailureCount, int eventChainLength)
    {
        RiskLevel adjusted = SeverityImpactRules.AdjustRiskLevel(baseScore.OverallRiskLevel, eventSeverity, employeeInjuryCount, machineFailureCount, eventChainLength);

        string notes = adjusted == baseScore.OverallRiskLevel
            ? $"Baseline {baseScore.OverallRiskLevel} risk level unaffected by {eventSeverity} severity event."
            : $"Baseline {baseScore.OverallRiskLevel} risk level escalated to {adjusted} due to {eventSeverity} severity event (injuries: {employeeInjuryCount}, failures: {machineFailureCount}, chain length: {eventChainLength}).";

        return new SeverityAwareRiskScore(
            baseScore.FacilityId,
            baseScore,
            eventSeverity,
            employeeInjuryCount,
            machineFailureCount,
            eventChainLength,
            adjusted,
            notes);
    }
}
