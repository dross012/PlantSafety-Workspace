// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Domain
//  File:           DataLakeActivityEventId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for Intelligence.DataLake domain events.
//                  Mirrors PlantSafety.Domain.ContinuousImprovement.RCA.ValueObjects.Ids
//                  .RCAEventId's identical precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.DataLake.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Intelligence.DataLake domain events.</summary>
public readonly record struct DataLakeActivityEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DataLakeActivityEventId wrapping a non-empty GUID.</summary>
    public DataLakeActivityEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DataLakeActivityEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DataLakeActivityEventId using Uuid7.</summary>
    public static DataLakeActivityEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
