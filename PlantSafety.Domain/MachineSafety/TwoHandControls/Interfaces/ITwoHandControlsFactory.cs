// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.TwoHandControls
//  Layer:          Domain
//  File:           ITwoHandControlsFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing TwoHandControls aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.TwoHandControls.Interfaces;

/// <summary>Factory interface for constructing <see cref="TwoHandControls"/> aggregates.</summary>
public interface ITwoHandControlsFactory
{
    /// <summary>Creates a new TwoHandControls aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.MachineSafety.TwoHandControls.Aggregates.TwoHandControls Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}