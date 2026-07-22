// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.MachineGuardsBarriers
//  Layer:          Domain
//  File:           MachineGuardsBarriersSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for MachineGuardsBarriers aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Snapshots;

/// <summary>Immutable snapshot of a <see cref="MachineGuardsBarriers"/> aggregate for fast rehydration.</summary>
public sealed record MachineGuardsBarriersSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public MachineGuardsBarriersId MachineGuardsBarriersId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public MachineGuardsBarriersStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}