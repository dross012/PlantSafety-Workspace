// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Domain
//  File:           IPermitWorkflowRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PermitWorkflow aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitWorkflow.Interfaces;

/// <summary>Domain repository interface for the <see cref="PermitWorkflow"/> aggregate root.</summary>
public interface IPermitWorkflowRepository
{
    /// <summary>Retrieves a PermitWorkflow aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow?> GetByIdAsync(PermitWorkflowId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PermitWorkflow aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow>> ListAsync(PermitWorkflowSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PermitWorkflow aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing PermitWorkflow aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow aggregate, CancellationToken cancellationToken = default);
}