// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.StopWorkEvent
//  Layer:          Domain
//  File:           IStopWorkEventFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing StopWorkEvent aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.StopWorkEvent.Interfaces;

/// <summary>Factory interface for constructing <see cref="StopWorkEvent"/> aggregates.</summary>
public interface IStopWorkEventFactory
{
    /// <summary>Creates a new StopWorkEvent aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}