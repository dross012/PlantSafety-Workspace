// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Domain
//  File:           PermitsNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Permits.Exceptions;

/// <summary>Thrown when a <see cref="Permits"/> aggregate lookup finds no match.</summary>
public sealed class PermitsNotFoundException : PermitsDomainException
{
    /// <summary>Creates a not-found exception for the given Permits identity.</summary>
    public PermitsNotFoundException(PermitsId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "Permits with id '{id}' was not found.",
            "Permits.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
