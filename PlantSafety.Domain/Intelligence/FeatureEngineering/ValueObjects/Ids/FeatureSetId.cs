// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Domain
//  File:           FeatureSetId.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the Intelligence.FeatureEngineering
//                  FeatureSet entity. Ported from the orphaned, dead
//                  Intelligence.Domain/FeatureEngineering/Entities/FeatureSet.cs (previously a
//                  plain Guid Id on an Entity&lt;Guid&gt; base) into this module's own
//                  strong-typed-id convention. Mirrors
//                  PlantSafety.Domain.Intelligence.DataLake.ValueObjects.Ids.DataLakeActivityId's
//                  identical precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects.Ids;

/// <summary>Strong-typed identity for the Intelligence.FeatureEngineering FeatureSet entity.</summary>
public readonly record struct FeatureSetId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a FeatureSetId wrapping a non-empty GUID.</summary>
    public FeatureSetId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("FeatureSetId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new FeatureSetId using Uuid7.</summary>
    public static FeatureSetId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a FeatureSetId.</summary>
    public static FeatureSetId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
