// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           IAdvancedMetricsLedgerRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the AdvancedMetricsLedger aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.AdvancedMetricsLedger"/> aggregate root.</summary>
public interface IAdvancedMetricsLedgerRepository
{
    /// <summary>Retrieves an AdvancedMetricsLedger aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.AdvancedMetricsLedger?> GetByIdAsync(AdvancedMetricsLedgerId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the open ledger for the given tenant/site/reporting period, or <see langword="null"/>
    /// if none has been opened yet. Used by record-metric handlers to get-or-create the period's ledger.
    /// </summary>
    Task<Aggregates.AdvancedMetricsLedger?> GetOpenLedgerAsync(
        Guid tenantId, Guid? siteId, UtcTimestamp reportingPeriodStartUtc, UtcTimestamp reportingPeriodEndUtc,
        CancellationToken cancellationToken = default);

    /// <summary>Returns all AdvancedMetricsLedger aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Aggregates.AdvancedMetricsLedger>> ListAsync(Specifications.AdvancedMetricsLedgerSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new AdvancedMetricsLedger aggregate.</summary>
    Task AddAsync(Aggregates.AdvancedMetricsLedger aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing AdvancedMetricsLedger aggregate.</summary>
    Task UpdateAsync(Aggregates.AdvancedMetricsLedger aggregate, CancellationToken cancellationToken = default);
}
