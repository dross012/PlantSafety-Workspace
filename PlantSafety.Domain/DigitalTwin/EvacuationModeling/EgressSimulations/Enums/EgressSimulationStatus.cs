// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
//  Layer:          Domain
//  File:           EgressSimulationStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the EgressSimulationRun aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.EgressSimulationRun"/> aggregate root.</summary>
public enum EgressSimulationStatus
{
    /// <summary>Run in progress.</summary>
    Running = 0,

    /// <summary>Run completed successfully with results recorded.</summary>
    Completed = 1,

    /// <summary>Run failed before results could be computed.</summary>
    Failed = 2,
}
