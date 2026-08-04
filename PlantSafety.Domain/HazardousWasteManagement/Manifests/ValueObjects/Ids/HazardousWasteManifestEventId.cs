// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Manifests
//  Layer:          Domain
//  File:           HazardousWasteManifestEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Strong-typed, Uuid7-backed identity for HazardousWasteManifest domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.Manifests.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for HazardousWasteManifest domain events.</summary>
public readonly record struct HazardousWasteManifestEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a HazardousWasteManifestEventId wrapping a non-empty GUID.</summary>
    public HazardousWasteManifestEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("HazardousWasteManifestEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new HazardousWasteManifestEventId using Uuid7.</summary>
    public static HazardousWasteManifestEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
