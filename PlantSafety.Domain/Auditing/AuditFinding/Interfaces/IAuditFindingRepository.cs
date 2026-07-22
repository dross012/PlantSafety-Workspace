// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding
//  Layer:          Domain
//  File:           IAuditFindingRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the AuditFinding aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditFinding.Interfaces;

/// <summary>Domain repository interface for the <see cref="AuditFinding"/> aggregate root.</summary>
public interface IAuditFindingRepository
{
    /// <summary>Retrieves a AuditFinding aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Auditing.AuditFinding.Aggregates.AuditFinding?> GetByIdAsync(AuditFindingId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all AuditFinding aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Auditing.AuditFinding.Aggregates.AuditFinding>> ListAsync(AuditFindingSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new AuditFinding aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Auditing.AuditFinding.Aggregates.AuditFinding aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing AuditFinding aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Auditing.AuditFinding.Aggregates.AuditFinding aggregate, CancellationToken cancellationToken = default);
}