// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Training
//  Layer:          Domain
//  File:           TrainingSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Training aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Training.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Training"/> aggregate for fast rehydration.</summary>
public sealed record TrainingSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public TrainingId TrainingId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public TrainingStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
