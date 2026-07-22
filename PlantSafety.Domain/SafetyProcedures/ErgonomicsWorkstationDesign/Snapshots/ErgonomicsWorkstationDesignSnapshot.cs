// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
//  Layer:          Domain
//  File:           ErgonomicsWorkstationDesignSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for ErgonomicsWorkstationDesign aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Snapshots;

/// <summary>Immutable snapshot of a <see cref="ErgonomicsWorkstationDesign"/> aggregate for fast rehydration.</summary>
public sealed record ErgonomicsWorkstationDesignSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public ErgonomicsWorkstationDesignId ErgonomicsWorkstationDesignId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public ErgonomicsWorkstationDesignStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}