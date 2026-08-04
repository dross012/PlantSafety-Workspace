// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessPoints
//  Layer:          Domain
//  File:           AccessPointStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the AccessPoint aggregate root (a badge reader,
//                  turnstile, mantrap door, or vehicle gate integrated with the physical
//                  access-control system).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Enums;

/// <summary>Lifecycle states for the <see cref="AccessPoint"/> aggregate root.</summary>
public enum AccessPointStatus
{
    /// <summary>Access point has been registered but not yet placed into service.</summary>
    Registered = 0,

    /// <summary>Access point is active and ingesting access events.</summary>
    Active = 1,

    /// <summary>Access point is offline for scheduled or unscheduled maintenance.</summary>
    Maintenance = 2,

    /// <summary>Access point has been permanently taken out of service.</summary>
    Decommissioned = 3,
}
