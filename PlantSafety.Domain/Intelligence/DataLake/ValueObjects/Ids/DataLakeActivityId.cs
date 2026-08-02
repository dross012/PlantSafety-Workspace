// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Domain
//  File:           DataLakeActivityId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the Intelligence.DataLake
//                  DataLakeActivity aggregate root. Mirrors
//                  PlantSafety.Domain.ContinuousImprovement.RCA.ValueObjects.Ids.RCAId's
//                  identical precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.DataLake.ValueObjects.Ids;

/// <summary>Strong-typed identity for the Intelligence.DataLake DataLakeActivity aggregate root.</summary>
public readonly record struct DataLakeActivityId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DataLakeActivityId wrapping a non-empty GUID.</summary>
    public DataLakeActivityId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DataLakeActivityId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DataLakeActivityId using Uuid7.</summary>
    public static DataLakeActivityId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a DataLakeActivityId.</summary>
    public static DataLakeActivityId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
