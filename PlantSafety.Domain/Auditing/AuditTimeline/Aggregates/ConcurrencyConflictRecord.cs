// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Domain
//  File:           ConcurrencyConflictRecord.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root capturing a single optimistic-concurrency write failure
//                  (ETag mismatch / zero-rows-affected) so it is durably persisted and appears
//                  in AuditTimeline reconstruction, rather than being silently dropped as a
//                  transient exception (Handbook Phase 5 §6.1). Infrastructure repository
//                  implementations that catch ConcurrencyConflictException on an aggregate
//                  UpdateAsync path should also create-and-persist one of these records.
//                  Additive-only evolution.
// ============================================================================
using System.Collections.ObjectModel;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditTimeline.Events;
using PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects.Ids;

namespace PlantSafety.Domain.Auditing.AuditTimeline.Aggregates;

/// <summary>Aggregate root capturing a single optimistic concurrency conflict for audit purposes.</summary>
public sealed class ConcurrencyConflictRecord
{
    /// <summary>Strong-typed identity of this conflict record.</summary>
    public ConcurrencyConflictRecordId ConcurrencyConflictRecordId { get; private set; }

    /// <summary>Tenant the conflicting entity belongs to.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Type discriminator of the entity/aggregate the conflicting write targeted.</summary>
    public string EntityType { get; private set; } = string.Empty;

    /// <summary>Identity of the entity/aggregate the conflicting write targeted.</summary>
    public Guid EntityId { get; private set; }

    /// <summary>ETag/concurrency token the caller supplied.</summary>
    public string ExpectedETag { get; private set; } = string.Empty;

    /// <summary>ETag/concurrency token actually present in storage at conflict time, if known.</summary>
    public string? ActualETag { get; private set; }

    /// <summary>Operation that was attempted (e.g. "UpdateAsync").</summary>
    public string AttemptedOperation { get; private set; } = string.Empty;

    /// <summary>Identity of the actor who attempted the write.</summary>
    public Guid? ActorId { get; private set; }

    /// <summary>UTC instant the conflict was detected.</summary>
    public UtcTimestamp OccurredAtUtc { get; private set; }

    /// <summary>Correlation identifier of the operation that triggered the conflict.</summary>
    public Guid CorrelationId { get; private set; }

    private readonly List<IAuditTimelineDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAuditTimelineDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAuditTimelineDomainEvent>(_domainEvents);

    // For rehydration only.
    private ConcurrencyConflictRecord() { }

    /// <summary>Captures a new concurrency conflict and raises <see cref="ConcurrencyConflictRecordedEvent"/>.</summary>
    public static ConcurrencyConflictRecord Capture(
        Guid tenantId, string entityType, Guid entityId, string expectedETag, string? actualETag,
        string attemptedOperation, Guid? actorId, UtcTimestamp occurredAtUtc,
        PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        var record = new ConcurrencyConflictRecord
        {
            ConcurrencyConflictRecordId = ConcurrencyConflictRecordId.New(),
            TenantId = tenantId,
            EntityType = (entityType ?? string.Empty).Trim(),
            EntityId = entityId,
            ExpectedETag = (expectedETag ?? string.Empty).Trim(),
            ActualETag = actualETag,
            AttemptedOperation = (attemptedOperation ?? string.Empty).Trim(),
            ActorId = actorId,
            OccurredAtUtc = occurredAtUtc,
            CorrelationId = correlationId.Value,
        };

        record._domainEvents.Add(new ConcurrencyConflictRecordedEvent(
            Uuid7.New(), record.ConcurrencyConflictRecordId, record.EntityType, record.EntityId,
            record.ExpectedETag, record.ActualETag, record.AttemptedOperation, occurredAtUtc,
            actor, correlationId, causationId));

        return record;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
