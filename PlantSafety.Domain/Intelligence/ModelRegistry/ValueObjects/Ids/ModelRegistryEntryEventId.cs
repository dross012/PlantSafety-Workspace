// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Domain
//  File:           ModelRegistryEntryEventId.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for Intelligence.ModelRegistry domain
//                  events. Mirrors
//                  PlantSafety.Domain.Intelligence.DataLake.ValueObjects.Ids.DataLakeActivityEventId's
//                  identical precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Intelligence.ModelRegistry domain events.</summary>
public readonly record struct ModelRegistryEntryEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ModelRegistryEntryEventId wrapping a non-empty GUID.</summary>
    public ModelRegistryEntryEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ModelRegistryEntryEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ModelRegistryEntryEventId using Uuid7.</summary>
    public static ModelRegistryEntryEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
