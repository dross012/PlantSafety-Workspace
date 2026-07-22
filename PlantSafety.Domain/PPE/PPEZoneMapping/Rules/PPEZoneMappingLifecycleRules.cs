// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEZoneMapping
//  Layer:          Domain
//  File:           PPEZoneMappingLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the PPEZoneMapping aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEZoneMapping.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="PPEZoneMapping"/> aggregate.</summary>
public static class PPEZoneMappingLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(PPEZoneMappingStatus current) => current == PPEZoneMappingStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(PPEZoneMappingStatus current) => current == PPEZoneMappingStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(PPEZoneMappingStatus current) => current == PPEZoneMappingStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(PPEZoneMappingStatus current) => current == PPEZoneMappingStatus.Closed;
}