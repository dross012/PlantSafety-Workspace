// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Domain
//  File:           IObservationFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing Observation aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.Observation.Interfaces;

/// <summary>Factory interface for constructing <see cref="Observation"/> aggregates.</summary>
public interface IObservationFactory
{
    /// <summary>Creates a new Observation aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
