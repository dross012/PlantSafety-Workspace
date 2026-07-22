// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
//  Layer:          Domain
//  File:           ErgonomicsAndMaterialHandlingSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for ErgonomicsAndMaterialHandling aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Snapshots;

/// <summary>Immutable snapshot of a <see cref="ErgonomicsAndMaterialHandling"/> aggregate for fast rehydration.</summary>
public sealed record ErgonomicsAndMaterialHandlingSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public ErgonomicsAndMaterialHandlingId ErgonomicsAndMaterialHandlingId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public ErgonomicsAndMaterialHandlingStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}