// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Explicit, deterministic static mapping between AuditTimeline domain
//   ValueObjects/Aggregates and Application-layer DTOs. See ComplianceEvaluationMapper remarks
//   for why this codebase uses explicit static mappers rather than Mapster/reflection mapping.
// SchemaVersion: 1

using PlantSafety.Domain.Auditing.AuditTimeline.Aggregates;
using PlantSafety.Domain.Auditing.AuditTimeline.Enums;
using PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects;
using PlantSafety.Application.Auditing.AuditTimeline.Models;

namespace PlantSafety.Application.Auditing.AuditTimeline.Mapping;

/// <summary>Explicit static mapper between AuditTimeline domain types and Application DTOs.</summary>
public static class AuditTimelineMapper
{
    /// <summary>Maps a single domain <see cref="AuditTimelineEntry"/> to its DTO representation.</summary>
    public static AuditTimelineEntryDto ToDto(AuditTimelineEntry entry)
        => new()
        {
            AuditTimelineEntryId = entry.AuditTimelineEntryId.Value,
            EntryType = entry.EntryType.Code,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            Summary = entry.Summary,
            DetailJson = entry.DetailJson,
            ActorId = entry.ActorId,
            UtcTimestamp = entry.UtcTimestamp.Value,
            CorrelationId = entry.CorrelationId,
        };

    /// <summary>Maps a captured <see cref="ConcurrencyConflictRecord"/> aggregate to a ConcurrencyConflict AuditTimelineEntry.</summary>
    public static AuditTimelineEntry ToTimelineEntry(ConcurrencyConflictRecord record)
        => AuditTimelineEntry.Create(
            AuditTimelineEntryType.ConcurrencyConflict,
            record.EntityType,
            record.EntityId,
            $"Concurrency conflict on '{record.AttemptedOperation}': expected ETag '{record.ExpectedETag}', actual '{record.ActualETag ?? "unknown"}'.",
            record.OccurredAtUtc,
            detailJson: null,
            actorId: record.ActorId,
            correlationId: record.CorrelationId);
}
