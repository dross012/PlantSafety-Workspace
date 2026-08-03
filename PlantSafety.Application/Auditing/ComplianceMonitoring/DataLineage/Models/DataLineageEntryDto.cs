// AUTHOR: PlantSafety Platform Team (Claude Code performance-optimization-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: A single event-chain entry in a data-lineage reconstruction (Handbook
//                    Phase 6 §5.4 Compliance Monitoring -- "which system/event chain produced
//                    this record"). Composed from the EXISTING Outbox ([System].[OutboxMessages]
//                    via the AuditTimeline module's usp_OutboxMessages_GetByTenant) and
//                    DeadLetterEvents ([System].[DeadLetterEvents]) infrastructure -- not a new
//                    heavyweight lineage engine.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.ComplianceMonitoring.DataLineage.Models;

/// <summary>Canonical classification of a data-lineage chain entry's origin.</summary>
public enum DataLineageEntryKind
{
    /// <summary>The event was successfully published onto the outbox/event-mesh pipeline.</summary>
    EventPublished = 0,

    /// <summary>The event permanently failed delivery and was recorded to the dead-letter queue.</summary>
    EventDeadLettered = 1
}

/// <summary>A single entry in a reconstructed cross-system event/data-lineage chain.</summary>
/// <param name="Kind">Whether this entry represents a successful publish or a dead-lettered fault.</param>
/// <param name="EventType">Canonical event-type discriminator.</param>
/// <param name="SchemaVersion">Event envelope schema edition.</param>
/// <param name="OccurredAtUtc">UTC instant the event was raised (published) or faulted (dead-lettered).</param>
/// <param name="CorrelationId">Propagated correlation identifier tying this entry to the requested chain.</param>
/// <param name="CausationId">Propagated causation identifier identifying the immediate parent operation, if known.</param>
/// <param name="ActorId">Actor identifier recovered from the envelope, if present.</param>
/// <param name="Description">Human-readable summary of this lineage entry.</param>
public sealed record DataLineageEntryDto(
    DataLineageEntryKind Kind,
    string EventType,
    string SchemaVersion,
    DateTime OccurredAtUtc,
    Guid? CorrelationId,
    Guid? CausationId,
    string? ActorId,
    string Description);
