// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           BatchId.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for a Batch produced within a
//                  ProductionRun. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;

/// <summary>Strong-typed identity for a Batch produced within a ProductionRun.</summary>
public readonly record struct BatchId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a BatchId wrapping a non-empty GUID.</summary>
    public BatchId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("BatchId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new BatchId using Uuid7.</summary>
    public static BatchId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a BatchId.</summary>
    public static BatchId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
