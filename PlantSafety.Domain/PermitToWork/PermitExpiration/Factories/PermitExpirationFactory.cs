// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitExpiration
//  Layer:          Domain
//  File:           PermitExpirationFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PermitExpiration aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitExpiration.Factories;

/// <summary>Default factory implementation for constructing <see cref="PermitExpiration"/> aggregates.</summary>
public sealed class PermitExpirationFactory : IPermitExpirationFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}