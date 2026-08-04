// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.AccumulationAreas
//  Layer:          Domain
//  File:           AccumulationAreaSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Snapshot record for AccumulationArea aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Enums;
using PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Snapshots;

/// <summary>Immutable snapshot of an AccumulationArea aggregate for fast rehydration.</summary>
public sealed record AccumulationAreaSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public AccumulationAreaId AccumulationAreaId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public AccumulationAreaStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
