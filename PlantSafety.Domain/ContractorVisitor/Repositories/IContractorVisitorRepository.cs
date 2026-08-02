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
//  UPDATED:        2026-08-02 -- Phase 4 full build-out: GetByIdAsync now takes a tenantId
//                  parameter (tenant-scoped from the start) and UpdateAsync now takes an
//                  optimistic-concurrency ETag.
// ============================================================================
namespace PlantSafety.Domain.ContractorVisitor.Repositories;

/// <summary>Domain repository interface for the <see cref="ContractorVisitor"/> aggregate root.</summary>
public interface IContractorVisitorRepository
{
    /// <summary>Retrieves a ContractorVisitor aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor?> GetByIdAsync(ContractorVisitorId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new ContractorVisitor aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing ContractorVisitor aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">The last-known concurrency token (from a prior read). A stale or mismatched value causes the persistence layer to reject the write as a concurrency conflict.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor aggregate, string? eTag, CancellationToken cancellationToken = default);
}
