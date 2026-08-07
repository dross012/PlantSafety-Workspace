// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Domain
//  File:           InvalidDOTSpecException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when a candidate DOT packaging spec code does not match the
//                  canonical alphanumeric format (2-8 characters, e.g. "1A1", "31HA1").
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Packaging.Exceptions;

/// <summary>Thrown when a candidate DOT packaging spec code does not match the expected format.</summary>
public sealed class InvalidDOTSpecException : PackagingSpecificationDomainException
{
    /// <summary>Creates an invalid-DOT-spec exception for the supplied raw value.</summary>
    public InvalidDOTSpecException(string? value, CorrelationId correlationId, CausationId causationId)
        : base(
            $"'{value}' is not a valid DOT packaging spec code. Expected 2-8 alphanumeric characters (e.g. '1A1', '4G', '31HA1').",
            "PackagingSpecification.InvalidDOTSpec",
            "Validation",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
