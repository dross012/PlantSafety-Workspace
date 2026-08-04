// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           TaskComplexityProfileId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the TaskComplexityProfile child entity.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the TaskComplexityProfile child entity.</summary>
public readonly record struct TaskComplexityProfileId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a TaskComplexityProfileId wrapping a non-empty GUID.</summary>
    public TaskComplexityProfileId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TaskComplexityProfileId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new TaskComplexityProfileId using Uuid7.</summary>
    public static TaskComplexityProfileId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a TaskComplexityProfileId.</summary>
    public static TaskComplexityProfileId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a TaskComplexityProfileId.</summary>
    public static TaskComplexityProfileId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
