// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Domain
//  File:           PackagingSpecificationLifecycleRules.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Guards for the PackagingSpecification aggregate's authorization lifecycle.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Packaging.Enums;

namespace PlantSafety.Domain.Compliance.PHMSA.Packaging.Rules;

/// <summary>Defines permitted lifecycle transitions for the <c>PackagingSpecification</c> aggregate.</summary>
public static class PackagingSpecificationLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be superseded from <paramref name="current"/>.</summary>
    public static bool CanSupersede(PackagingSpecificationStatus current) => current == PackagingSpecificationStatus.Authorized;

    /// <summary>Returns <see langword="true"/> when the aggregate may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(PackagingSpecificationStatus current) => current != PackagingSpecificationStatus.Retired;
}
