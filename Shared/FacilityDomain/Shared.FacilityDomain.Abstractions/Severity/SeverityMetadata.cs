// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Severity
//  File:           SeverityMetadata.cs
//  Created:        2026-08-08
//  Description:    Phase C.3: static, deterministic metadata describing
//                  each SeverityLevel -- a numeric weight (used by
//                  Phase C.10's severity-aware compliance/risk scoring)
//                  and a human-readable description. Mirrors the precedent
//                  set by Shared.SafetyData's static lookup catalogs (e.g.
//                  OrchestrationEventCategoryCatalog).
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.Severity;

/// <summary>Static, deterministic metadata describing a <see cref="SeverityLevel"/>.</summary>
public static class SeverityMetadata
{
    /// <summary>The numeric weight of each severity level, ascending with severity. Used for deterministic score adjustments.</summary>
    public static int WeightOf(SeverityLevel level) => level switch
    {
        SeverityLevel.Minor => 1,
        SeverityLevel.Moderate => 2,
        SeverityLevel.Severe => 3,
        SeverityLevel.Catastrophic => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unknown SeverityLevel."),
    };

    /// <summary>A human-readable description of the severity level.</summary>
    public static string DescriptionOf(SeverityLevel level) => level switch
    {
        SeverityLevel.Minor => "No significant injury, damage, or regulatory exposure.",
        SeverityLevel.Moderate => "Limited injury/damage; recordable but contained.",
        SeverityLevel.Severe => "Significant injury/damage; likely reportable to a regulator.",
        SeverityLevel.Catastrophic => "Life-threatening, fatal, or facility-wide consequence.",
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unknown SeverityLevel."),
    };
}
