// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitTemplate
//  Layer:          Domain
//  File:           IPermitTemplateRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PermitTemplate aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitTemplate.Interfaces;

/// <summary>Domain repository interface for the <see cref="PermitTemplate"/> aggregate root.</summary>
public interface IPermitTemplateRepository
{
    /// <summary>Retrieves a PermitTemplate aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate?> GetByIdAsync(PermitTemplateId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PermitTemplate aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate>> ListAsync(PermitTemplateSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PermitTemplate aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing PermitTemplate aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate aggregate, CancellationToken cancellationToken = default);
}