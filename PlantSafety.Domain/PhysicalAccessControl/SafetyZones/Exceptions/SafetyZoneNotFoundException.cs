// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.SafetyZones
//  Layer:          Domain
//  File:           SafetyZoneNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a SafetyZone aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Exceptions;

/// <summary>Thrown when a <see cref="SafetyZone"/> aggregate lookup finds no match.</summary>
public sealed class SafetyZoneNotFoundException : SafetyZoneDomainException
{
    /// <summary>Creates a not-found exception for the given SafetyZone identity.</summary>
    public SafetyZoneNotFoundException(SafetyZoneId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"SafetyZone with id '{id}' was not found.",
            "SafetyZone.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
