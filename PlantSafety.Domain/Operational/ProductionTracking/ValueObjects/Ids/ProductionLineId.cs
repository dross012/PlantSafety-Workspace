// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           ProductionLineId.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the ProductionLine (master-data)
//                  aggregate root. New-build module backing the ProductionTracking UI
//                  (v1/production-tracking/*), which previously had no Application/API/Domain
//                  layer at all (see docs/Phase5/ui-gaps-scoping.md "ProductionTracking" section).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;

/// <summary>Strong-typed identity for the ProductionLine aggregate root.</summary>
public readonly record struct ProductionLineId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ProductionLineId wrapping a non-empty GUID.</summary>
    public ProductionLineId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProductionLineId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ProductionLineId using Uuid7.</summary>
    public static ProductionLineId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ProductionLineId.</summary>
    public static ProductionLineId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
