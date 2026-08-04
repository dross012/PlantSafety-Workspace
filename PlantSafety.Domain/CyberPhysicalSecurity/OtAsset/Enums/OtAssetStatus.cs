// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.OtAsset
//  Layer:          Domain
//  File:           OtAssetStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle status of an OtAsset inventory record.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Enums;

/// <summary>Lifecycle status of an OtAsset inventory record.</summary>
public enum OtAssetStatus
{
    /// <summary>Registered and actively monitored.</summary>
    Active = 0,

    /// <summary>Flagged for security review (e.g., unreconciled, stale firmware).</summary>
    UnderReview = 1,

    /// <summary>Isolated from the OT network pending investigation.</summary>
    Quarantined = 2,

    /// <summary>Removed from service.</summary>
    Decommissioned = 3
}
