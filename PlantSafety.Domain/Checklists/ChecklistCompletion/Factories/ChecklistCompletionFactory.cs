// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Domain
//  File:           ChecklistCompletionFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing ChecklistCompletion aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistCompletion.Factories;

/// <summary>Default factory implementation for constructing <see cref="ChecklistCompletion"/> aggregates.</summary>
public sealed class ChecklistCompletionFactory : IChecklistCompletionFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}