// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MaterialHandlingStorage
//  Layer:          Domain
//  File:           MaterialHandlingStorageId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the MaterialHandlingStorage aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the MaterialHandlingStorage aggregate root.</summary>
public readonly record struct MaterialHandlingStorageId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a MaterialHandlingStorageId wrapping a non-empty GUID.</summary>
    public MaterialHandlingStorageId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MaterialHandlingStorageId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new MaterialHandlingStorageId using Uuid7.</summary>
    public static MaterialHandlingStorageId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a MaterialHandlingStorageId.</summary>
    public static MaterialHandlingStorageId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a MaterialHandlingStorageId.</summary>
    public static MaterialHandlingStorageId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}