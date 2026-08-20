// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Domain
//  File:           ModelRegistryEntryId.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the Intelligence.ModelRegistry
//                  ModelRegistryEntry aggregate root. Ported from the orphaned, dead
//                  Intelligence.Domain/ModelRegistry/Entities/ModelRegistryEntry.cs (previously a
//                  plain Guid Id on an AggregateRoot<Guid> base) into this module's own
//                  strong-typed-id convention. Mirrors
//                  PlantSafety.Domain.Intelligence.DataLake.ValueObjects.Ids.DataLakeActivityId's
//                  identical precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects.Ids;

/// <summary>Strong-typed identity for the Intelligence.ModelRegistry ModelRegistryEntry aggregate root.</summary>
public readonly record struct ModelRegistryEntryId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ModelRegistryEntryId wrapping a non-empty GUID.</summary>
    public ModelRegistryEntryId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ModelRegistryEntryId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ModelRegistryEntryId using Uuid7.</summary>
    public static ModelRegistryEntryId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ModelRegistryEntryId.</summary>
    public static ModelRegistryEntryId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
