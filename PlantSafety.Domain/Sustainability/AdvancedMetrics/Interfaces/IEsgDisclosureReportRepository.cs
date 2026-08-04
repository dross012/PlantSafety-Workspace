// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           IEsgDisclosureReportRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the EsgDisclosureReport aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.EsgDisclosureReport"/> aggregate root.</summary>
public interface IEsgDisclosureReportRepository
{
    /// <summary>Retrieves an EsgDisclosureReport aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.EsgDisclosureReport?> GetByIdAsync(EsgDisclosureReportId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all EsgDisclosureReport aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Aggregates.EsgDisclosureReport>> ListAsync(Specifications.EsgDisclosureReportSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new EsgDisclosureReport aggregate.</summary>
    Task AddAsync(Aggregates.EsgDisclosureReport aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing EsgDisclosureReport aggregate.</summary>
    Task UpdateAsync(Aggregates.EsgDisclosureReport aggregate, CancellationToken cancellationToken = default);
}
