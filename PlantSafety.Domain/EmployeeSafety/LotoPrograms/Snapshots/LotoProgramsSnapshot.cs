// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.LotoPrograms
//  Layer:          Domain
//  File:           LotoProgramsSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for LotoPrograms aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.LotoPrograms.Snapshots;

/// <summary>Immutable snapshot of a <see cref="LotoPrograms"/> aggregate for fast rehydration.</summary>
public sealed record LotoProgramsSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public LotoProgramsId LotoProgramsId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public LotoProgramsStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}