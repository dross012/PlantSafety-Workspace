// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PermitToWork
//  Layer:          Domain
//  File:           PermitToWorkFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PermitToWork aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PermitToWork.Factories;

/// <summary>Default factory implementation for constructing <see cref="PermitToWork"/> aggregates.</summary>
public sealed class PermitToWorkFactory : IPermitToWorkFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.ContractorSafety.PermitToWork.Aggregates.PermitToWork Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.ContractorSafety.PermitToWork.Aggregates.PermitToWork.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}