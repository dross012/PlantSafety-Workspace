// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           EvacuationRouteId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the EvacuationRoute aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the EvacuationRoute aggregate root.</summary>
public readonly record struct EvacuationRouteId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an EvacuationRouteId wrapping a non-empty GUID.</summary>
    public EvacuationRouteId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EvacuationRouteId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EvacuationRouteId using Uuid7.</summary>
    public static EvacuationRouteId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an EvacuationRouteId.</summary>
    public static EvacuationRouteId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into an EvacuationRouteId.</summary>
    public static EvacuationRouteId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
