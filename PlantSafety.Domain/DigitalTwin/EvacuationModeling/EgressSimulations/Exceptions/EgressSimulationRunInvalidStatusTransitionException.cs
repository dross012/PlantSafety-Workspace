// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
//  Layer:          Domain
//  File:           EgressSimulationRunInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an EgressSimulationRun lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Exceptions;

/// <summary>Thrown when an <see cref="Aggregates.EgressSimulationRun"/> lifecycle transition is not permitted.</summary>
public sealed class EgressSimulationRunInvalidStatusTransitionException : EgressSimulationRunDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public EgressSimulationRunInvalidStatusTransitionException(
        EgressSimulationRunId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'EgressSimulationRun' {id} from '{from}' to '{to}'.",
            "EgressSimulationRun.InvalidTransition", "Lifecycle", "Medium",
            correlationId, causationId)
    {
    }
}
