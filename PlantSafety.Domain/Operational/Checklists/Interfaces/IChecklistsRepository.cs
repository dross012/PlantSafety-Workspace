// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Checklists
//  Layer:          Domain
//  File:           IChecklistsRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Checklists aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Checklists.Interfaces;

/// <summary>Domain repository interface for the <see cref="Checklists"/> aggregate root.</summary>
public interface IChecklistsRepository
{
    /// <summary>Retrieves a Checklists aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Checklists?> GetByIdAsync(ChecklistsId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Checklists aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Checklists>> ListAsync(ChecklistsSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Checklists aggregate.</summary>
    Task AddAsync(Checklists aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Checklists aggregate.</summary>
    Task UpdateAsync(Checklists aggregate, CancellationToken cancellationToken = default);
}
