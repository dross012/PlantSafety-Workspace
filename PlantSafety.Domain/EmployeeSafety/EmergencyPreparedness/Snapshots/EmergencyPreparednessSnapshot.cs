// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EmergencyPreparedness
//  Layer:          Domain
//  File:           EmergencyPreparednessSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for EmergencyPreparedness aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Snapshots;

/// <summary>Immutable snapshot of a <see cref="EmergencyPreparedness"/> aggregate for fast rehydration.</summary>
public sealed record EmergencyPreparednessSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public EmergencyPreparednessId EmergencyPreparednessId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public EmergencyPreparednessStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}