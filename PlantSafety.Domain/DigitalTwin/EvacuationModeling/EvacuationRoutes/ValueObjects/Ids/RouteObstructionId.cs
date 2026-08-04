// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           RouteObstructionId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for RouteObstruction entries.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a <see cref="Entities.RouteObstructionEntry"/>.</summary>
public readonly record struct RouteObstructionId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a RouteObstructionId wrapping a non-empty GUID.</summary>
    public RouteObstructionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RouteObstructionId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new RouteObstructionId using Uuid7.</summary>
    public static RouteObstructionId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a RouteObstructionId.</summary>
    public static RouteObstructionId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
