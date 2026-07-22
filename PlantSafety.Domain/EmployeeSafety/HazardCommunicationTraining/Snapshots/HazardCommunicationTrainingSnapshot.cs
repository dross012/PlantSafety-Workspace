// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.HazardCommunicationTraining
//  Layer:          Domain
//  File:           HazardCommunicationTrainingSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for HazardCommunicationTraining aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Snapshots;

/// <summary>Immutable snapshot of a <see cref="HazardCommunicationTraining"/> aggregate for fast rehydration.</summary>
public sealed record HazardCommunicationTrainingSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public HazardCommunicationTrainingId HazardCommunicationTrainingId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public HazardCommunicationTrainingStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}