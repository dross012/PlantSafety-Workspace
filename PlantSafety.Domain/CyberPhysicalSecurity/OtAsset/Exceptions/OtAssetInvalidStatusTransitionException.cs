// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.OtAsset
//  Layer:          Domain
//  File:           OtAssetInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when an OtAsset lifecycle transition is attempted from an invalid state.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Enums;

namespace PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Exceptions;

/// <summary>Raised when an OtAsset status transition is invalid.</summary>
public sealed class OtAssetInvalidStatusTransitionException : OtAssetDomainException
{
    /// <summary>Creates a new <see cref="OtAssetInvalidStatusTransitionException"/>.</summary>
    public OtAssetInvalidStatusTransitionException(OtAssetStatus from, OtAssetStatus to)
        : base($"Cannot transition OtAsset from '{from}' to '{to}'.") { }
}
