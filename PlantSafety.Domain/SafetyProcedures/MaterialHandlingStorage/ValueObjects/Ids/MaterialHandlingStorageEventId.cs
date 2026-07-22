// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MaterialHandlingStorage
//  Layer:          Domain
//  File:           MaterialHandlingStorageEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for MaterialHandlingStorage domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for MaterialHandlingStorage domain events.</summary>
public readonly record struct MaterialHandlingStorageEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a MaterialHandlingStorageEventId wrapping a non-empty GUID.</summary>
    public MaterialHandlingStorageEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MaterialHandlingStorageEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new MaterialHandlingStorageEventId using Uuid7.</summary>
    public static MaterialHandlingStorageEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}