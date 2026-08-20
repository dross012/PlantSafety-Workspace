// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Domain
//  File:           FeatureSetEventId.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for Intelligence.FeatureEngineering domain
//                  events. Mirrors
//                  PlantSafety.Domain.Intelligence.DataLake.ValueObjects.Ids.DataLakeActivityEventId's
//                  identical precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Intelligence.FeatureEngineering domain events.</summary>
public readonly record struct FeatureSetEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a FeatureSetEventId wrapping a non-empty GUID.</summary>
    public FeatureSetEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("FeatureSetEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new FeatureSetEventId using Uuid7.</summary>
    public static FeatureSetEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
