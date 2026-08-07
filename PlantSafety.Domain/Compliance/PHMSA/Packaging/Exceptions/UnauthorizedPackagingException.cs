// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Domain
//  File:           UnauthorizedPackagingException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown by callers of PackagingEngine.ValidatePackaging that choose to throw
//                  rather than inspect the returned PackagingValidationResult, when a proposed
//                  DOT packaging spec is not authorized for the given hazard class/packing group.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Packaging.Exceptions;

/// <summary>
/// Thrown when a proposed DOT packaging spec is not authorized for a given hazard class/packing
/// group combination. Callers of <c>PackagingEngine.ValidatePackaging</c> may throw this instead
/// of inspecting the returned result when they need fail-fast semantics.
/// </summary>
public sealed class UnauthorizedPackagingException : PackagingSpecificationDomainException
{
    /// <summary>Creates an unauthorized-packaging exception for the given spec/class/group combination.</summary>
    public UnauthorizedPackagingException(string proposedSpec, string hazardClass, string packingGroup, CorrelationId correlationId, CausationId causationId)
        : base(
            $"Packaging spec '{proposedSpec}' is not authorized for hazard class '{hazardClass}', packing group '{packingGroup}'.",
            "PackagingSpecification.Unauthorized",
            "Validation",
            "High",
            correlationId,
            causationId)
    {
    }
}
