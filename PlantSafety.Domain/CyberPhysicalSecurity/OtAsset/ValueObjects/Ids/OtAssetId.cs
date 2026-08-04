// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.OtAsset
//  Layer:          Domain
//  File:           OtAssetId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for the OtAsset aggregate root (OT Asset Inventory
//                  subdomain of CyberPhysicalSecurity).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the OtAsset aggregate root.</summary>
public readonly record struct OtAssetId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an OtAssetId wrapping a non-empty GUID.</summary>
    public OtAssetId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OtAssetId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new OtAssetId using Uuid7.</summary>
    public static OtAssetId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an OtAssetId.</summary>
    public static OtAssetId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into an OtAssetId.</summary>
    public static OtAssetId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
