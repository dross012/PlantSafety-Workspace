// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.SiteOrientation
//  Layer:          Domain
//  File:           SiteOrientationId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the SiteOrientation aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.SiteOrientation.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the SiteOrientation aggregate root.</summary>
public readonly record struct SiteOrientationId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SiteOrientationId wrapping a non-empty GUID.</summary>
    public SiteOrientationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SiteOrientationId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SiteOrientationId using Uuid7.</summary>
    public static SiteOrientationId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a SiteOrientationId.</summary>
    public static SiteOrientationId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a SiteOrientationId.</summary>
    public static SiteOrientationId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}