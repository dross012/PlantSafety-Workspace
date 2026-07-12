// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Checklists
//  Layer:          Domain
//  File:           IChecklistsFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing Checklists aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Checklists.Interfaces;

/// <summary>Factory interface for constructing <see cref="Checklists"/> aggregates.</summary>
public interface IChecklistsFactory
{
    /// <summary>Creates a new Checklists aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Operational.Checklists.Aggregates.Checklists Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
