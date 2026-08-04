// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           AssemblyPointId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for AssemblyPoint entities.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for an <see cref="Entities.AssemblyPoint"/>.</summary>
public readonly record struct AssemblyPointId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an AssemblyPointId wrapping a non-empty GUID.</summary>
    public AssemblyPointId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AssemblyPointId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AssemblyPointId using Uuid7.</summary>
    public static AssemblyPointId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an AssemblyPointId.</summary>
    public static AssemblyPointId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
