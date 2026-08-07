// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Domain
//  File:           PackagingSpecificationStatus.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the PackagingSpecification aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Packaging.Enums;

/// <summary>Lifecycle states for the <c>PackagingSpecification</c> aggregate root.</summary>
public enum PackagingSpecificationStatus
{
    /// <summary>Specification record has been started but not yet authorized.</summary>
    Draft = 0,

    /// <summary>Specification has been authorized under 49 CFR Part 178 and is the active record.</summary>
    Authorized = 1,

    /// <summary>Specification has been superseded by a newer authorization.</summary>
    Superseded = 2,

    /// <summary>Specification has been retired and is no longer authoritative.</summary>
    Retired = 3,
}
