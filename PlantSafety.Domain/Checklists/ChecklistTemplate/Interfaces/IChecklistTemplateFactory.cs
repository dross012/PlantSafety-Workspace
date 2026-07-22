// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           IChecklistTemplateFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing ChecklistTemplate aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.Interfaces;

/// <summary>Factory interface for constructing <see cref="ChecklistTemplate"/> aggregates.</summary>
public interface IChecklistTemplateFactory
{
    /// <summary>Creates a new ChecklistTemplate aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}