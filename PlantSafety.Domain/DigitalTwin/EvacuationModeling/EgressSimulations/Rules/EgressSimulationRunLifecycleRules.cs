// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
//  Layer:          Domain
//  File:           EgressSimulationRunLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the EgressSimulationRun aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.EgressSimulationRun"/> aggregate.</summary>
public static class EgressSimulationRunLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the run may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(EgressSimulationStatus current) => current == EgressSimulationStatus.Running;

    /// <summary>Returns <see langword="true"/> when the run may be marked failed from <paramref name="current"/>.</summary>
    public static bool CanFail(EgressSimulationStatus current) => current == EgressSimulationStatus.Running;

    /// <summary>Returns <see langword="true"/> when the run may be linked to a drill (any terminal or in-progress state).</summary>
    public static bool CanLinkToDrill(EgressSimulationStatus current) => true;
}
