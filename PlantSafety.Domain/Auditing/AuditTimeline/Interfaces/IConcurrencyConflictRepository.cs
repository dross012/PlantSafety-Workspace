// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Domain
//  File:           IConcurrencyConflictRepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the ConcurrencyConflictRecord aggregate
//                  root. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Auditing.AuditTimeline.Aggregates;
using PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects.Ids;

namespace PlantSafety.Domain.Auditing.AuditTimeline.Interfaces;

/// <summary>Domain repository interface for the ConcurrencyConflictRecord aggregate root.</summary>
public interface IConcurrencyConflictRepository
{
    /// <summary>Persists a newly captured concurrency conflict record.</summary>
    Task AddAsync(ConcurrencyConflictRecord record, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single conflict record by identity, scoped to the given tenant.</summary>
    Task<ConcurrencyConflictRecord?> GetByIdAsync(ConcurrencyConflictRecordId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all captured conflict records for a specific entity, most recent first.</summary>
    Task<IReadOnlyList<ConcurrencyConflictRecord>> GetForEntityAsync(Guid tenantId, string entityType, Guid entityId, CancellationToken cancellationToken = default);
}
