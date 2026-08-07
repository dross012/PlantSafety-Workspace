// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Domain
//  File:           PackagingSpecificationNotFoundException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PackagingSpecification aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Packaging.Exceptions;

/// <summary>Thrown when a <c>PackagingSpecification</c> aggregate lookup finds no match.</summary>
public sealed class PackagingSpecificationNotFoundException : PackagingSpecificationDomainException
{
    /// <summary>Creates a not-found exception for the given PackagingSpecification identity.</summary>
    public PackagingSpecificationNotFoundException(PackagingSpecificationId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"PackagingSpecification with id '{id}' was not found.",
            "PackagingSpecification.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
