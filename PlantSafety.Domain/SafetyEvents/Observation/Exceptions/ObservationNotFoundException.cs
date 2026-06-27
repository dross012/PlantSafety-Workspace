// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Domain
//  File:           ObservationNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.Observation.Exceptions;

/// <summary>Thrown when a <see cref="Observation"/> aggregate lookup finds no match.</summary>
public sealed class ObservationNotFoundException : ObservationDomainException
{
    /// <summary>Creates a not-found exception for the given Observation identity.</summary>
    public ObservationNotFoundException(ObservationId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "Observation with id '{id}' was not found.",
            "Observation.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
