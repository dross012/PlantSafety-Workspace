// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
//  Layer:          Domain
//  File:           EgressSimulationRunNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an EgressSimulationRun aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Exceptions;

/// <summary>Thrown when an <see cref="Aggregates.EgressSimulationRun"/> aggregate lookup finds no match.</summary>
public sealed class EgressSimulationRunNotFoundException : EgressSimulationRunDomainException
{
    /// <summary>Creates a not-found exception for the given EgressSimulationRun identity.</summary>
    public EgressSimulationRunNotFoundException(EgressSimulationRunId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"EgressSimulationRun with id '{id}' was not found.",
            "EgressSimulationRun.NotFound", "NotFound", "Medium",
            correlationId, causationId)
    {
    }
}
