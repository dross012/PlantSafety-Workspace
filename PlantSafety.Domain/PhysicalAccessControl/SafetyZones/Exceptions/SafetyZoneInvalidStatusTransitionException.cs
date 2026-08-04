// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.SafetyZones
//  Layer:          Domain
//  File:           SafetyZoneInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a SafetyZone lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Exceptions;

/// <summary>Thrown when a <see cref="SafetyZone"/> lifecycle transition is not permitted.</summary>
public sealed class SafetyZoneInvalidStatusTransitionException : SafetyZoneDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public SafetyZoneInvalidStatusTransitionException(
        SafetyZoneId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'SafetyZone' {id} from '{from}' to '{to}'.",
            "SafetyZone.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
