// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEZoneMapping
//  Layer:          Domain
//  File:           PPEZoneMappingArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for PPEZoneMapping aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEZoneMapping.Policies;

/// <summary>Determines whether a <see cref="PPEZoneMapping"/> aggregate is eligible for archival.</summary>
public static class PPEZoneMappingArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the PPEZoneMapping may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping aggregate)
        => aggregate.Status == PPEZoneMappingStatus.Closed;
}