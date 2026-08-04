// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           EvacuationRouteEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for EvacuationRoute domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for EvacuationRoute domain events.</summary>
public readonly record struct EvacuationRouteEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an EvacuationRouteEventId wrapping a non-empty GUID.</summary>
    public EvacuationRouteEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EvacuationRouteEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EvacuationRouteEventId using Uuid7.</summary>
    public static EvacuationRouteEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
