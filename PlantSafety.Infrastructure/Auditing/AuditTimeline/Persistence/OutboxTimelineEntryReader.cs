// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Infrastructure
//  File:           OutboxTimelineEntryReader.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Shared read helper over the cross-module [System].[OutboxMessages] event log
//                  (via [Auditing].[usp_OutboxMessages_GetByTenant]) used by
//                  AuditTimelineEventSourceOutboxRepository, AuditTimelineStateChangeOutboxRepository,
//                  and SchedulingDecisionOutboxProvider. Entity-scoping is performed in C# by a
//                  deterministic substring match of the target entity id's canonical GUID string
//                  ("D" format) against the raw JSON payload -- every domain event in this
//                  codebase carries its owning aggregate's strong-typed id as a payload property,
//                  and GUIDs are high-entropy enough that a substring match is a safe, simple,
//                  dependency-free correlation strategy that works uniformly across every module
//                  without per-module payload-shape knowledge. No dynamic SQL is used (Handbook
//                  §2.16B) -- all filtering happens in C# over the results of one fixed,
//                  parameterized, tenant-scoped stored procedure call. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Auditing.AuditTimeline.Enums;
using PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects;

namespace PlantSafety.Infrastructure.Auditing.AuditTimeline.Persistence;

/// <summary>Shared Outbox-backed read helper for AuditTimeline entry sources.</summary>
public sealed class OutboxTimelineEntryReader(DapperQueryExecutor queryExecutor, ILogger<OutboxTimelineEntryReader> logger)
{
    /// <summary>Returns every outbox row for the tenant whose payload references <paramref name="entityId"/>.</summary>
    public async Task<IReadOnlyList<OutboxRow>> GetRowsForEntityAsync(Guid tenantId, Guid entityId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddString("TenantId", tenantId.ToString(), 100).Build();

        var rows = await queryExecutor.QueryAsync<OutboxRow>(
            "[Auditing].[usp_OutboxMessages_GetByTenant]", parameters, cancellationToken);

        var needle = entityId.ToString("D");
        var matched = rows.Where(r => r.Payload.Contains(needle, StringComparison.OrdinalIgnoreCase)).ToList();

        logger.LogDebug("OutboxTimelineEntryReader: matched {MatchedCount}/{TotalCount} outbox rows for entity {EntityId}",
            matched.Count, rows.Count, entityId);

        return matched;
    }

    /// <summary>Maps a matched outbox row to a generic EventRaised AuditTimelineEntry.</summary>
    public static AuditTimelineEntry ToEventEntry(OutboxRow row, string entityType, Guid entityId)
        => AuditTimelineEntry.Create(
            AuditTimelineEntryType.EventRaised, entityType, entityId,
            $"Domain event '{row.EventType}' raised.", UtcTimestamp.From(row.OccurredAt),
            detailJson: row.Payload,
            actorId: Guid.TryParse(row.ActorId, out var actorId) ? actorId : null,
            correlationId: Guid.TryParse(row.CorrelationId, out var correlationId) ? correlationId : null);

    /// <summary>Maps a matched outbox row to a StateChange AuditTimelineEntry.</summary>
    public static AuditTimelineEntry ToStateChangeEntry(OutboxRow row, string entityType, Guid entityId)
        => AuditTimelineEntry.Create(
            AuditTimelineEntryType.StateChange, entityType, entityId,
            $"State change recorded via '{row.EventType}'.", UtcTimestamp.From(row.OccurredAt),
            detailJson: row.Payload,
            actorId: Guid.TryParse(row.ActorId, out var actorId) ? actorId : null,
            correlationId: Guid.TryParse(row.CorrelationId, out var correlationId) ? correlationId : null);

    /// <summary>Maps a matched outbox row to a SchedulingDecision AuditTimelineEntry.</summary>
    public static AuditTimelineEntry ToSchedulingDecisionEntry(OutboxRow row, string entityType, Guid entityId)
        => AuditTimelineEntry.Create(
            AuditTimelineEntryType.SchedulingDecision, entityType, entityId,
            $"Scheduling orchestration decision '{row.EventType}' recorded.", UtcTimestamp.From(row.OccurredAt),
            detailJson: row.Payload,
            actorId: Guid.TryParse(row.ActorId, out var actorId) ? actorId : null,
            correlationId: Guid.TryParse(row.CorrelationId, out var correlationId) ? correlationId : null);

    /// <summary>Flat read-model row shape returned by <c>usp_OutboxMessages_GetByTenant</c>.</summary>
    public sealed record OutboxRow
    {
        public required Guid MessageId { get; init; }
        public required string EventType { get; init; }
        public required string SchemaVersion { get; init; }
        public required string TenantId { get; init; }
        public required string ActorId { get; init; }
        public required string CorrelationId { get; init; }
        public required DateTime OccurredAt { get; init; }
        public required string Payload { get; init; }
    }
}
