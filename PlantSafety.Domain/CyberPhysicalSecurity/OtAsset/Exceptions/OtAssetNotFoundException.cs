// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.OtAsset
//  Layer:          Domain
//  File:           OtAssetNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when an OtAsset cannot be located by identity.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Exceptions;

/// <summary>Raised when an OtAsset cannot be located by identity.</summary>
public sealed class OtAssetNotFoundException : OtAssetDomainException
{
    /// <summary>Creates a new <see cref="OtAssetNotFoundException"/>.</summary>
    public OtAssetNotFoundException(Guid otAssetId)
        : base($"OtAsset '{otAssetId}' was not found.") { }
}
