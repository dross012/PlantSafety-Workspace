// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
//  Layer:          Domain
//  File:           EvacuationZoneInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an EvacuationZone lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Exceptions;

/// <summary>Thrown when an <see cref="Aggregates.EvacuationZone"/> lifecycle transition is not permitted.</summary>
public sealed class EvacuationZoneInvalidStatusTransitionException : EvacuationZoneDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public EvacuationZoneInvalidStatusTransitionException(
        EvacuationZoneId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'EvacuationZone' {id} from '{from}' to '{to}'.",
            "EvacuationZone.InvalidTransition", "Lifecycle", "Medium",
            correlationId, causationId)
    {
    }
}
