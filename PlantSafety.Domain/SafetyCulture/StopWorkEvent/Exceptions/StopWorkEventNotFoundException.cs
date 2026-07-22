// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.StopWorkEvent
//  Layer:          Domain
//  File:           StopWorkEventNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a StopWorkEvent aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.StopWorkEvent.Exceptions;

/// <summary>Thrown when a <see cref="StopWorkEvent"/> aggregate lookup finds no match.</summary>
public sealed class StopWorkEventNotFoundException : StopWorkEventDomainException
{
    /// <summary>Creates a not-found exception for the given StopWorkEvent identity.</summary>
    public StopWorkEventNotFoundException(StopWorkEventId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "StopWorkEvent with id '" + id + "' was not found.",
            "StopWorkEvent.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}