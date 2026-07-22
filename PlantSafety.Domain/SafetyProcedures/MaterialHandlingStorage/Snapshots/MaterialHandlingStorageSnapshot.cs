// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MaterialHandlingStorage
//  Layer:          Domain
//  File:           MaterialHandlingStorageSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for MaterialHandlingStorage aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Snapshots;

/// <summary>Immutable snapshot of a <see cref="MaterialHandlingStorage"/> aggregate for fast rehydration.</summary>
public sealed record MaterialHandlingStorageSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public MaterialHandlingStorageId MaterialHandlingStorageId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public MaterialHandlingStorageStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}