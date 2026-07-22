// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement
//  Layer:          Domain
//  File:           IAuditEngagementRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the AuditEngagement aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditEngagement.Interfaces;

/// <summary>Domain repository interface for the <see cref="AuditEngagement"/> aggregate root.</summary>
public interface IAuditEngagementRepository
{
    /// <summary>Retrieves a AuditEngagement aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement?> GetByIdAsync(AuditEngagementId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all AuditEngagement aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement>> ListAsync(AuditEngagementSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new AuditEngagement aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing AuditEngagement aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement aggregate, CancellationToken cancellationToken = default);
}