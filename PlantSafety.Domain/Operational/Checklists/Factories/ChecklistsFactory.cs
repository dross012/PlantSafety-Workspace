// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Checklists
//  Layer:          Domain
//  File:           ChecklistsFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Checklists aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Checklists.Factories;

/// <summary>Default factory implementation for constructing <see cref="Checklists"/> aggregates.</summary>
public sealed class ChecklistsFactory : IChecklistsFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Operational.Checklists.Aggregates.Checklists Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Operational.Checklists.Aggregates.Checklists.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
