// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
//  Layer:          Domain
//  File:           EgressSimulationTrigger.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Classification of what triggered an egress-simulation run. Mirrors the module's
//                  scheduling hooks: scheduled re-simulation on twin-model change, quarterly
//                  full-site simulation, and drill-triggered replay. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Enums;

/// <summary>Classification of what triggered an egress-simulation run.</summary>
public enum EgressSimulationTrigger
{
    /// <summary>Manually requested by an operator.</summary>
    Manual = 0,

    /// <summary>Automatically re-run because the underlying Digital Twin spatial model changed.</summary>
    TwinModelChangeResimulation = 1,

    /// <summary>Scheduled quarterly full-site simulation.</summary>
    QuarterlyFullSite = 2,

    /// <summary>Re-run as a replay correlated to an actual evacuation drill.</summary>
    DrillTriggeredReplay = 3,
}
