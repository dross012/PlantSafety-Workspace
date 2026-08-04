// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           WorkloadTrendRecordId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the WorkloadTrendRecord child entity.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the WorkloadTrendRecord child entity.</summary>
public readonly record struct WorkloadTrendRecordId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a WorkloadTrendRecordId wrapping a non-empty GUID.</summary>
    public WorkloadTrendRecordId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WorkloadTrendRecordId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new WorkloadTrendRecordId using Uuid7.</summary>
    public static WorkloadTrendRecordId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a WorkloadTrendRecordId.</summary>
    public static WorkloadTrendRecordId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a WorkloadTrendRecordId.</summary>
    public static WorkloadTrendRecordId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
