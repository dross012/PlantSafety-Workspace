// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.OtAsset
//  Layer:          Domain
//  File:           OtAssetDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Base exception type for all OtAsset domain rule violations.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Exceptions;

/// <summary>Base exception for OtAsset domain rule violations.</summary>
public class OtAssetDomainException : Exception
{
    /// <summary>Creates a new <see cref="OtAssetDomainException"/>.</summary>
    public OtAssetDomainException(string message) : base(message) { }

    /// <summary>Creates a new <see cref="OtAssetDomainException"/> with an inner exception.</summary>
    public OtAssetDomainException(string message, Exception innerException) : base(message, innerException) { }
}
