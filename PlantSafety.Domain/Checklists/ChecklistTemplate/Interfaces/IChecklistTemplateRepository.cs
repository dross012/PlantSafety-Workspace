// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           IChecklistTemplateRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the ChecklistTemplate aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.Interfaces;

/// <summary>Domain repository interface for the <see cref="ChecklistTemplate"/> aggregate root.</summary>
public interface IChecklistTemplateRepository
{
    /// <summary>Retrieves a ChecklistTemplate aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate?> GetByIdAsync(ChecklistTemplateId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all ChecklistTemplate aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate>> ListAsync(ChecklistTemplateSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new ChecklistTemplate aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing ChecklistTemplate aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate aggregate, CancellationToken cancellationToken = default);
}