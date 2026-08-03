// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           IAuditReportRepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the AuditReport aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Auditing.AuditReport.ValueObjects.Ids;

namespace PlantSafety.Domain.Auditing.AuditReport.Interfaces;

/// <summary>Domain repository interface for the AuditReport aggregate root.</summary>
public interface IAuditReportRepository
{
    /// <summary>Retrieves an AuditReport aggregate by identity, scoped to the given tenant.</summary>
    Task<Aggregates.AuditReport?> GetByIdAsync(AuditReportId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns the most recent AuditReport aggregates generated for a specific target entity.</summary>
    Task<IReadOnlyList<Aggregates.AuditReport>> GetForTargetAsync(Guid tenantId, string targetEntityType, Guid targetEntityId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new AuditReport aggregate (Draft status).</summary>
    Task AddAsync(Aggregates.AuditReport aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists the outcome (Generated or Failed) of an existing AuditReport aggregate.</summary>
    Task UpdateAsync(Aggregates.AuditReport aggregate, CancellationToken cancellationToken = default);
}
