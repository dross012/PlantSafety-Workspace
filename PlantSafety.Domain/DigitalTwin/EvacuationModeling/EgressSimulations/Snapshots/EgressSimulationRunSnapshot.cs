// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
//  Layer:          Domain
//  File:           EgressSimulationRunSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for EgressSimulationRun aggregate rehydration. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Snapshots;

/// <summary>Immutable snapshot of an <see cref="Aggregates.EgressSimulationRun"/> aggregate for fast rehydration.</summary>
public sealed record EgressSimulationRunSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public EgressSimulationRunId EgressSimulationRunId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public EgressSimulationStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
