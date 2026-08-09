// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Regulatory
//  File:           RegulatoryJurisdiction.cs
//  Created:        2026-08-08
//  Description:    Phase C.4 (Path C: Operational Modernization): the
//                  unified regulatory-jurisdiction vocabulary, placed
//                  alongside HazardType/SeverityLevel since all three are
//                  shared, additive-only classification vocabularies.
//                  Additive-only: new members may be appended, existing
//                  members must never be renumbered or removed once
//                  published.
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.Regulatory;

/// <summary>The unified regulatory-jurisdiction vocabulary.</summary>
public enum RegulatoryJurisdiction
{
    /// <summary>U.S. Occupational Safety and Health Administration.</summary>
    OSHA,

    /// <summary>U.S. Environmental Protection Agency.</summary>
    EPA,

    /// <summary>U.S. Department of Transportation.</summary>
    DOT,

    /// <summary>National Fire Protection Association.</summary>
    NFPA,

    /// <summary>A state-level regulatory body.</summary>
    State,

    /// <summary>A local/municipal regulatory body.</summary>
    Local,
}
