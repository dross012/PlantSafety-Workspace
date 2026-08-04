// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessPoints
//  Layer:          Domain
//  File:           AccessPointLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the AccessPoint aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="AccessPoint"/> aggregate.</summary>
public static class AccessPointLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the access point may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(AccessPointStatus current) =>
        current is AccessPointStatus.Registered or AccessPointStatus.Maintenance;

    /// <summary>Returns <see langword="true"/> when maintenance may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleMaintenance(AccessPointStatus current) => current == AccessPointStatus.Active;

    /// <summary>Returns <see langword="true"/> when the access point may be decommissioned from <paramref name="current"/>.</summary>
    public static bool CanDecommission(AccessPointStatus current) =>
        current is AccessPointStatus.Registered or AccessPointStatus.Active or AccessPointStatus.Maintenance;
}
