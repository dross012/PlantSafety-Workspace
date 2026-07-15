// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Domain
//  File:           IProcessDefinitionRepository.cs
//  Created:        2026-07-15
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the ProcessDefinition aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ProcessManagement.Repositories;

/// <summary>Domain repository interface for the <see cref="PlantSafety.Domain.ProcessManagement.Aggregates.ProcessDefinition"/> aggregate root.</summary>
public interface IProcessDefinitionRepository
{
    /// <summary>Retrieves a ProcessDefinition aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.ProcessManagement.Aggregates.ProcessDefinition?> GetByIdAsync(ProcessDefinitionId id, CancellationToken cancellationToken = default);

    /// <summary>Persists a new ProcessDefinition aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.ProcessManagement.Aggregates.ProcessDefinition aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing ProcessDefinition aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.ProcessManagement.Aggregates.ProcessDefinition aggregate, CancellationToken cancellationToken = default);
}
