// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Domain
//  File:           IFinancialRecordRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface (write-side) for the new FinancialRecord
//                  aggregate root. A brand-new file -- there is no pre-existing Domain-layer
//                  repository interface for Financials today, only the pre-existing, unrelated,
//                  Application-layer read-only IFinancialsRepository
//                  (PlantSafety.Application.Financials.Financials.Repositories.IFinancialsRepository),
//                  which is left completely untouched. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Interfaces.IAuditingRepository.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Financials.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.FinancialRecord"/> aggregate root.</summary>
public interface IFinancialRecordRepository
{
    /// <summary>Retrieves a FinancialRecord aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.FinancialRecord?> GetByIdAsync(FinancialsId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new FinancialRecord aggregate.</summary>
    Task AddAsync(Aggregates.FinancialRecord aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing FinancialRecord aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(Aggregates.FinancialRecord aggregate, string? eTag, CancellationToken cancellationToken = default);
}
