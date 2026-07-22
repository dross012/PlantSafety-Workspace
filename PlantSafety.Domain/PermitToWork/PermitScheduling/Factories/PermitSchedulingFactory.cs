// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitScheduling
//  Layer:          Domain
//  File:           PermitSchedulingFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PermitScheduling aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitScheduling.Factories;

/// <summary>Default factory implementation for constructing <see cref="PermitScheduling"/> aggregates.</summary>
public sealed class PermitSchedulingFactory : IPermitSchedulingFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}