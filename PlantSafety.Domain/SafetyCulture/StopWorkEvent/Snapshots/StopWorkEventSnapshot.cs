// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.StopWorkEvent
//  Layer:          Domain
//  File:           StopWorkEventSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for StopWorkEvent aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.StopWorkEvent.Snapshots;

/// <summary>Immutable snapshot of a <see cref="StopWorkEvent"/> aggregate for fast rehydration.</summary>
public sealed record StopWorkEventSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public StopWorkEventId StopWorkEventId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public StopWorkEventStatus Status { get; init; }

    /// <summary>Work area/location at snapshot time.</summary>
    public string WorkArea { get; init; } = string.Empty;

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}