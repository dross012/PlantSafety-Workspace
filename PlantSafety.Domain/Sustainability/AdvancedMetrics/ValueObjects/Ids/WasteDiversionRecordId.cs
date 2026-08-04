// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           WasteDiversionRecordId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for a WasteDiversionRecord child entity.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a WasteDiversionRecord child entity.</summary>
public readonly record struct WasteDiversionRecordId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a WasteDiversionRecordId wrapping a non-empty GUID.</summary>
    public WasteDiversionRecordId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WasteDiversionRecordId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new WasteDiversionRecordId using Uuid7.</summary>
    public static WasteDiversionRecordId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
