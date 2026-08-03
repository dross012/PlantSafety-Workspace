// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           ProductionRunId.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the ProductionRun aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;

/// <summary>Strong-typed identity for the ProductionRun aggregate root.</summary>
public readonly record struct ProductionRunId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ProductionRunId wrapping a non-empty GUID.</summary>
    public ProductionRunId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProductionRunId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ProductionRunId using Uuid7.</summary>
    public static ProductionRunId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ProductionRunId.</summary>
    public static ProductionRunId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
