// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Domain
//  File:           IChecklistCompletionRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the ChecklistCompletion aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistCompletion.Interfaces;

/// <summary>Domain repository interface for the <see cref="ChecklistCompletion"/> aggregate root.</summary>
public interface IChecklistCompletionRepository
{
    /// <summary>Retrieves a ChecklistCompletion aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion?> GetByIdAsync(ChecklistCompletionId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all ChecklistCompletion aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion>> ListAsync(ChecklistCompletionSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new ChecklistCompletion aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing ChecklistCompletion aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion aggregate, CancellationToken cancellationToken = default);
}