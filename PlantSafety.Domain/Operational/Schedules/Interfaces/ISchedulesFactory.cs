// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Schedules
//  Layer:          Domain
//  File:           ISchedulesFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing Schedules aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Schedules.Interfaces;

/// <summary>Factory interface for constructing <see cref="Schedules"/> aggregates.</summary>
public interface ISchedulesFactory
{
    /// <summary>Creates a new Schedules aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Operational.Schedules.Aggregates.Schedules Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
