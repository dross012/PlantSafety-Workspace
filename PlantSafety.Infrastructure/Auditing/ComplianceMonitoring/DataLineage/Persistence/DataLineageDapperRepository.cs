// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.ComplianceMonitoring.DataLineage
//  Layer:          Infrastructure
//  File:           DataLineageDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code performance-optimization-agent)
//  SchemaVersion:  2026.1
//  Description:    Composes the EXISTING [Auditing].[usp_OutboxMessages_GetByTenant] stored
//                  procedure (already used by AuditTimeline's OutboxTimelineEntryReader) with the
//                  EXISTING IDeadLetterEventRepository.GetByCorrelationIdAsync to reconstruct a
//                  cross-system event chain for a given CorrelationId -- Handbook Phase 6 §5.4
//                  Compliance Monitoring's "which system/event chain produced this record", built
//                  as a composition over existing data, NOT a new heavyweight lineage engine.
//                  Entity/correlation scoping mirrors OutboxTimelineEntryReader's established
//                  approach: a deterministic C#-side string match against the raw JSON payload's
//                  CorrelationId column (no dynamic SQL, Handbook §2.16B). Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Application.Auditing.ComplianceMonitoring.DataLineage.Models;
using PlantSafety.Application.Auditing.ComplianceMonitoring.DataLineage.Repositories;
using PlantSafety.Application.Messaging.Repositories;

namespace PlantSafety.Infrastructure.Auditing.ComplianceMonitoring.DataLineage.Persistence;

/// <summary>
/// Dapper-backed <see cref="IDataLineageRepository"/> composing the existing Outbox and
/// DeadLetterEvents read paths.
/// </summary>
public sealed class DataLineageDapperRepository(
    DapperQueryExecutor queryExecutor,
    IDeadLetterEventRepository deadLetterEventRepository,
    ILogger<DataLineageDapperRepository> logger) : IDataLineageRepository
{
    /// <summary>Flat read-model row shape returned by <c>usp_OutboxMessages_GetByTenant</c> (mirrors OutboxTimelineEntryReader.OutboxRow).</summary>
    private sealed record OutboxRow
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

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DataLineageEntryDto>> GetLineageAsync(
        Guid tenantId, Guid correlationId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetLineageAsync for tenant {TenantId} correlation {CorrelationId}", tenantId, correlationId);

        var outboxParameters = new StoredProcedureBuilder().AddString("TenantId", tenantId.ToString(), 100).Build();

        var outboxRows = await queryExecutor.QueryAsync<OutboxRow>(
            "[Auditing].[usp_OutboxMessages_GetByTenant]", outboxParameters, cancellationToken);

        var publishedEntries = outboxRows
            .Where(r => string.Equals(r.CorrelationId, correlationId.ToString(), StringComparison.OrdinalIgnoreCase))
            .Select(r => new DataLineageEntryDto(
                Kind: DataLineageEntryKind.EventPublished,
                EventType: r.EventType,
                SchemaVersion: r.SchemaVersion,
                OccurredAtUtc: r.OccurredAt,
                CorrelationId: correlationId,
                CausationId: null,
                ActorId: r.ActorId,
                Description: $"Event '{r.EventType}' published to the outbox/event-mesh pipeline."));

        var deadLetterRows = await deadLetterEventRepository.GetByCorrelationIdAsync(correlationId, cancellationToken);

        var deadLetteredEntries = deadLetterRows.Select(d => new DataLineageEntryDto(
            Kind: DataLineageEntryKind.EventDeadLettered,
            EventType: d.EventType,
            SchemaVersion: d.SchemaVersion,
            OccurredAtUtc: d.FaultedAtUtc,
            CorrelationId: d.CorrelationId,
            CausationId: d.CausationId,
            ActorId: d.ActorId,
            Description: d.IsReplayed
                ? $"Event '{d.EventType}' dead-lettered ({d.FaultReason}), later replayed."
                : $"Event '{d.EventType}' dead-lettered ({d.FaultReason})."));

        return publishedEntries
            .Concat(deadLetteredEntries)
            .OrderBy(e => e.OccurredAtUtc)
            .ToList();
    }
}
