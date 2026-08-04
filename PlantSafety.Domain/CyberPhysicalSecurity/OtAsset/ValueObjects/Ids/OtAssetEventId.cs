// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.OtAsset
//  Layer:          Domain
//  File:           OtAssetEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for OtAsset domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for OtAsset domain events.</summary>
public readonly record struct OtAssetEventId(Guid Value)
{
    /// <summary>Creates a new OtAssetEventId using Uuid7.</summary>
    public static OtAssetEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
