// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Domain
//  File:           PackagingSpecificationInvalidStatusTransitionException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PackagingSpecification lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Packaging.Exceptions;

/// <summary>Thrown when a <c>PackagingSpecification</c> lifecycle transition is not permitted.</summary>
public sealed class PackagingSpecificationInvalidStatusTransitionException : PackagingSpecificationDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public PackagingSpecificationInvalidStatusTransitionException(
        PackagingSpecificationId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition PackagingSpecification '{id}' from '{from}' to '{to}'.",
            "PackagingSpecification.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
