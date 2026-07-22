// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPETrainingFit
//  Layer:          Domain
//  File:           PPETrainingFitSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for PPETrainingFit aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPETrainingFit.Snapshots;

/// <summary>Immutable snapshot of a <see cref="PPETrainingFit"/> aggregate for fast rehydration.</summary>
public sealed record PPETrainingFitSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PPETrainingFitId PPETrainingFitId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PPETrainingFitStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}