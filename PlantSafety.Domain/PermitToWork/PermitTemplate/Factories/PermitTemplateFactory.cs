// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitTemplate
//  Layer:          Domain
//  File:           PermitTemplateFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PermitTemplate aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitTemplate.Factories;

/// <summary>Default factory implementation for constructing <see cref="PermitTemplate"/> aggregates.</summary>
public sealed class PermitTemplateFactory : IPermitTemplateFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}