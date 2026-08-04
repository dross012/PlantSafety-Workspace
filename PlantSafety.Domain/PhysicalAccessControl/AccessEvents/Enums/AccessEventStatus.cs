// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessEvents
//  Layer:          Domain
//  File:           AccessEventStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Processing lifecycle states for the AccessEvent aggregate root (a single
//                  badge swipe/scan ingested from an external badge-system webhook or IoT feed).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Enums;

/// <summary>Processing lifecycle states for the <see cref="AccessEvent"/> aggregate root.</summary>
public enum AccessEventStatus
{
    /// <summary>Event has been ingested from the external badge system but not yet processed.</summary>
    Ingested = 0,

    /// <summary>Event has been correlated against zone occupancy / anomaly detection.</summary>
    Processed = 1,

    /// <summary>Event was flagged as anomalous (tailgating, forced entry, after-hours, restricted-zone violation).</summary>
    Flagged = 2,
}
