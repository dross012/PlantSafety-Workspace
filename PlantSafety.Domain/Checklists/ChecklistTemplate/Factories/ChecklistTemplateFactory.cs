// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           ChecklistTemplateFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing ChecklistTemplate aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.Factories;

/// <summary>Default factory implementation for constructing <see cref="ChecklistTemplate"/> aggregates.</summary>
public sealed class ChecklistTemplateFactory : IChecklistTemplateFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}