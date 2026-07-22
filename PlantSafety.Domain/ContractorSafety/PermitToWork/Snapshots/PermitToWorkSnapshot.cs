// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PermitToWork
//  Layer:          Domain
//  File:           PermitToWorkSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for PermitToWork aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PermitToWork.Snapshots;

/// <summary>Immutable snapshot of a <see cref="PermitToWork"/> aggregate for fast rehydration.</summary>
public sealed record PermitToWorkSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PermitToWorkId PermitToWorkId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PermitToWorkStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}