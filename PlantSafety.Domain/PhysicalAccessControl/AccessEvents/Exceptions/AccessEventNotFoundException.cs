// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessEvents
//  Layer:          Domain
//  File:           AccessEventNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an AccessEvent aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Exceptions;

/// <summary>Thrown when an <see cref="AccessEvent"/> aggregate lookup finds no match.</summary>
public sealed class AccessEventNotFoundException : AccessEventDomainException
{
    /// <summary>Creates a not-found exception for the given AccessEvent identity.</summary>
    public AccessEventNotFoundException(AccessEventId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"AccessEvent with id '{id}' was not found.",
            "AccessEvent.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
