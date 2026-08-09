// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.ComplianceRisk
//  Layer:          Abstractions
//  File:           SeverityImpactRules.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase C.10 (Path C: Operational Modernization):
//                  deterministic rules for adjusting a facility's existing
//                  ComplianceScore/RiskScore based on a real safety event's
//                  severity, employee injury count, machine failure count,
//                  and multi-hazard event-chain length. SeverityLevel below
//                  is a deliberate LOCAL duplicate of Shared.FacilityDomain's
//                  Phase C.3 SeverityLevel enum (Minor/Moderate/Severe/
//                  Catastrophic), not a reference to it -- Shared.SafetyData
//                  has zero cross-repo project references by design (see
//                  Shared.EventBus's EventEnvelopeV2 / this project's own
//                  TenantFacilityMap precedent), so this vocabulary is kept
//                  as a same-shaped local copy rather than importing the
//                  real type. Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;

/// <summary>A safety event's severity, shaped to match Shared.FacilityDomain's Phase C.3 SeverityLevel.</summary>
public enum SeverityLevel
{
    /// <summary>No significant injury, damage, or regulatory exposure.</summary>
    Minor,

    /// <summary>Limited injury/damage; recordable but contained.</summary>
    Moderate,

    /// <summary>Significant injury/damage; likely reportable to a regulator.</summary>
    Severe,

    /// <summary>Life-threatening, fatal, or facility-wide consequence.</summary>
    Catastrophic,
}

/// <summary>Deterministic rules for adjusting compliance/risk scores based on severity, injury, failure, and event-chain context.</summary>
public static class SeverityImpactRules
{
    /// <summary>The minimum <see cref="RiskLevel"/> a given severity implies, regardless of the facility's baseline risk.</summary>
    public static RiskLevel SeverityFloor(SeverityLevel severity) => severity switch
    {
        SeverityLevel.Minor => RiskLevel.Low,
        SeverityLevel.Moderate => RiskLevel.Medium,
        SeverityLevel.Severe => RiskLevel.High,
        SeverityLevel.Catastrophic => RiskLevel.Critical,
        _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, "Unknown SeverityLevel."),
    };

    /// <summary>True if the combination of injury/failure/chain-length context independently warrants escalating risk by one tier.</summary>
    public static bool RequiresEscalation(int employeeInjuryCount, int machineFailureCount, int eventChainLength) =>
        employeeInjuryCount >= 2 || machineFailureCount >= 2 || eventChainLength >= 3;

    /// <summary>
    /// Adjusts a baseline <see cref="RiskLevel"/>: raised to at least <see cref="SeverityFloor"/>, then escalated one
    /// further tier (capped at Critical) if <see cref="RequiresEscalation"/> holds. Never lowers the baseline.
    /// </summary>
    public static RiskLevel AdjustRiskLevel(RiskLevel baseline, SeverityLevel severity, int employeeInjuryCount, int machineFailureCount, int eventChainLength)
    {
        RiskLevel floor = SeverityFloor(severity);
        RiskLevel adjusted = baseline >= floor ? baseline : floor;

        if (RequiresEscalation(employeeInjuryCount, machineFailureCount, eventChainLength) && adjusted < RiskLevel.Critical)
        {
            adjusted += 1;
        }

        return adjusted;
    }

    /// <summary>
    /// Adjusts a baseline <see cref="ComplianceTier"/>: a Catastrophic event caps the tier at
    /// PartiallyCompliant, since a facility cannot be considered substantially/fully compliant
    /// immediately following a catastrophic event, regardless of its measured percent-complete.
    /// </summary>
    public static ComplianceTier AdjustComplianceTier(ComplianceTier baseline, SeverityLevel severity) =>
        severity == SeverityLevel.Catastrophic && baseline > ComplianceTier.PartiallyCompliant
            ? ComplianceTier.PartiallyCompliant
            : baseline;
}
