// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Schedules
//  Layer:          Domain
//  File:           SchedulesSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Schedules aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Schedules.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Schedules"/> aggregate for fast rehydration.</summary>
public sealed record SchedulesSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public SchedulesId SchedulesId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public SchedulesStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
