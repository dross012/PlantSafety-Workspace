// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
//  Layer:          Domain
//  File:           EvacuationZoneNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an EvacuationZone aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Exceptions;

/// <summary>Thrown when an <see cref="Aggregates.EvacuationZone"/> aggregate lookup finds no match.</summary>
public sealed class EvacuationZoneNotFoundException : EvacuationZoneDomainException
{
    /// <summary>Creates a not-found exception for the given EvacuationZone identity.</summary>
    public EvacuationZoneNotFoundException(EvacuationZoneId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"EvacuationZone with id '{id}' was not found.",
            "EvacuationZone.NotFound", "NotFound", "Medium",
            correlationId, causationId)
    {
    }
}
