// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Domain
//  File:           IContractorVisitorRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the ContractorVisitor aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorVisitor.Repositories;

/// <summary>Domain repository interface for the <see cref="ContractorVisitor"/> aggregate root.</summary>
public interface IContractorVisitorRepository
{
    /// <summary>Retrieves a ContractorVisitor aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<ContractorVisitor?> GetByIdAsync(ContractorVisitorId id, CancellationToken cancellationToken = default);

    /// <summary>Persists a new ContractorVisitor aggregate.</summary>
    Task AddAsync(ContractorVisitor aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing ContractorVisitor aggregate.</summary>
    Task UpdateAsync(ContractorVisitor aggregate, CancellationToken cancellationToken = default);
}
