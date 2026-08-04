// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.CraneHoistSystems
//  Layer:          Domain
//  File:           CraneHoistSystemsSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Snapshot record for CraneHoistSystems aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.CraneHoistSystems.Snapshots;

/// <summary>Immutable snapshot of a <see cref="CraneHoistSystems"/> aggregate for fast rehydration.</summary>
public sealed record CraneHoistSystemsSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public CraneHoistSystemsId CraneHoistSystemsId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public CraneHoistSystemsStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
