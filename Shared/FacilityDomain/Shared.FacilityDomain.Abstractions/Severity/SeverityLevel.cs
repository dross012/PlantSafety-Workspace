// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Severity
//  File:           SeverityLevel.cs
//  Created:        2026-08-08
//  Description:    Phase C.3 (Path C: Operational Modernization): the
//                  unified severity vocabulary, placed alongside HazardType
//                  since both are shared, additive-only classification
//                  vocabularies consumed across every domain. Additive-only:
//                  new members may be appended, existing members must never
//                  be renumbered or removed once published.
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.Severity;

/// <summary>The unified severity vocabulary, ordered from least to most severe.</summary>
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
