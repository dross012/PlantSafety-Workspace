// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Domain
//  File:           DashboardSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Dashboard aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Dashboard.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Dashboard"/> aggregate for fast rehydration.</summary>
public sealed record DashboardSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public DashboardId DashboardId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public DashboardStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
