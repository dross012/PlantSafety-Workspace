// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.SiteOrientation
//  Layer:          Domain
//  File:           SiteOrientationEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for SiteOrientation domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.SiteOrientation.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for SiteOrientation domain events.</summary>
public readonly record struct SiteOrientationEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SiteOrientationEventId wrapping a non-empty GUID.</summary>
    public SiteOrientationEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SiteOrientationEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SiteOrientationEventId using Uuid7.</summary>
    public static SiteOrientationEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}