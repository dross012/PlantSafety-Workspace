// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Domain
//  File:           IChecklistCompletionFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing ChecklistCompletion aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistCompletion.Interfaces;

/// <summary>Factory interface for constructing <see cref="ChecklistCompletion"/> aggregates.</summary>
public interface IChecklistCompletionFactory
{
    /// <summary>Creates a new ChecklistCompletion aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}