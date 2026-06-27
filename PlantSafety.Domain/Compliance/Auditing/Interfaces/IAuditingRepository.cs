// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Domain
//  File:           IAuditingRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Auditing aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Auditing.Interfaces;

/// <summary>Domain repository interface for the <see cref="Auditing"/> aggregate root.</summary>
public interface IAuditingRepository
{
    /// <summary>Retrieves a Auditing aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Auditing?> GetByIdAsync(AuditingId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Auditing aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Auditing>> ListAsync(AuditingSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Auditing aggregate.</summary>
    Task AddAsync(Auditing aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Auditing aggregate.</summary>
    Task UpdateAsync(Auditing aggregate, CancellationToken cancellationToken = default);
}
