// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitScheduling
//  Layer:          Domain
//  File:           IPermitSchedulingFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PermitScheduling aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitScheduling.Interfaces;

/// <summary>Factory interface for constructing <see cref="PermitScheduling"/> aggregates.</summary>
public interface IPermitSchedulingFactory
{
    /// <summary>Creates a new PermitScheduling aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}