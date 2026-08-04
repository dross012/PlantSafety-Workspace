// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.OtAsset
//  Layer:          Domain
//  File:           OtAssetCriticality.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Safety/production criticality rating of an OT asset, used to prioritize
//                  anomaly detection and incident response.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Enums;

/// <summary>Safety/production criticality rating of an OT asset.</summary>
public enum OtAssetCriticality
{
    /// <summary>Low criticality; limited safety or production impact if compromised.</summary>
    Low = 0,

    /// <summary>Medium criticality.</summary>
    Medium = 1,

    /// <summary>High criticality; direct production impact if compromised.</summary>
    High = 2,

    /// <summary>Critical; direct physical-safety impact if compromised (safety-instrumented asset).</summary>
    Critical = 3
}
