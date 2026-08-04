// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           ICognitiveLoadLedgerRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the CognitiveLoadLedger aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.CognitiveLoadLedger"/> aggregate root.</summary>
public interface ICognitiveLoadLedgerRepository
{
    /// <summary>Retrieves a CognitiveLoadLedger aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.CognitiveLoadLedger?> GetByIdAsync(CognitiveLoadLedgerId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the open ledger for the given tenant/site/shift, or <see langword="null"/> if none
    /// has been opened yet. Used by record-reading/define-profile handlers to get-or-create the
    /// shift's ledger.
    /// </summary>
    Task<Aggregates.CognitiveLoadLedger?> GetOpenLedgerAsync(
        Guid tenantId, Guid? siteId, ShiftType shiftType, UtcTimestamp shiftDateUtc,
        CancellationToken cancellationToken = default);

    /// <summary>Returns all CognitiveLoadLedger aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Aggregates.CognitiveLoadLedger>> ListAsync(Specifications.CognitiveLoadLedgerSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new CognitiveLoadLedger aggregate.</summary>
    Task AddAsync(Aggregates.CognitiveLoadLedger aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing CognitiveLoadLedger aggregate.</summary>
    Task UpdateAsync(Aggregates.CognitiveLoadLedger aggregate, CancellationToken cancellationToken = default);
}
