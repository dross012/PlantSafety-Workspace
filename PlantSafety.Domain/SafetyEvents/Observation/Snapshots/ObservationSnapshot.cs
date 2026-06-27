// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Domain
//  File:           ObservationSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Observation aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.Observation.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Observation"/> aggregate for fast rehydration.</summary>
public sealed record ObservationSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public ObservationId ObservationId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public ObservationStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
