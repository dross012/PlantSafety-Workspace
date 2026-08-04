// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteContainers
//  Layer:          Domain
//  File:           WasteContainerSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Snapshot record for WasteContainer aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Enums;
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Snapshots;

/// <summary>Immutable snapshot of a WasteContainer aggregate for fast rehydration.</summary>
public sealed record WasteContainerSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public WasteContainerId WasteContainerId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public WasteContainerStatus Status { get; init; }

    /// <summary>UTC timestamp the container was opened.</summary>
    public UtcTimestamp OpenedAtUtc { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
