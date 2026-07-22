// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Domain
//  File:           IPMSchedulingFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PMScheduling aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PMScheduling.Interfaces;

/// <summary>Factory interface for constructing <see cref="PMScheduling"/> aggregates.</summary>
public interface IPMSchedulingFactory
{
    /// <summary>Creates a new PMScheduling aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}