// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Domain
//  File:           IHazardIsolationSequencingFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing HazardIsolationSequencing aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Interfaces;

/// <summary>Factory interface for constructing <see cref="HazardIsolationSequencing"/> aggregates.</summary>
public interface IHazardIsolationSequencingFactory
{
    /// <summary>Creates a new HazardIsolationSequencing aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}