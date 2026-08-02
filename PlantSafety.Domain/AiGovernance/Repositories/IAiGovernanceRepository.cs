// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Domain
//  File:           IAiGovernanceRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the AiGovernance aggregate root.
//                  Additive-only evolution.
//  UPDATED:        2026-08-02 -- Phase 4 full build-out: GetByIdAsync now takes a tenantId
//                  parameter (tenant-scoped from the start) and UpdateAsync now takes an
//                  optimistic-concurrency ETag, mirroring IAuditingRepository's identical retrofit.
// ============================================================================
namespace PlantSafety.Domain.AiGovernance.Repositories;

/// <summary>Domain repository interface for the <see cref="AiGovernance"/> aggregate root.</summary>
public interface IAiGovernanceRepository
{
    /// <summary>Retrieves a AiGovernance aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance?> GetByIdAsync(AiGovernanceId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new AiGovernance aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing AiGovernance aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">The last-known concurrency token (from a prior read). A stale or mismatched value causes the persistence layer to reject the write as a concurrency conflict.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance aggregate, string? eTag, CancellationToken cancellationToken = default);
}
