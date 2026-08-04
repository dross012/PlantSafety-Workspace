// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Manifests
//  Layer:          Domain
//  File:           HazardousWasteManifestId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Strong-typed, Uuid7-backed identity for the HazardousWasteManifest aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.Manifests.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the HazardousWasteManifest aggregate root.</summary>
public readonly record struct HazardousWasteManifestId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a HazardousWasteManifestId wrapping a non-empty GUID.</summary>
    public HazardousWasteManifestId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("HazardousWasteManifestId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new HazardousWasteManifestId using Uuid7.</summary>
    public static HazardousWasteManifestId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a HazardousWasteManifestId.</summary>
    public static HazardousWasteManifestId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a HazardousWasteManifestId.</summary>
    public static HazardousWasteManifestId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
