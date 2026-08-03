// AUTHOR: PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Read-side projection of a single row from [System].[DeadLetterEvents] --
//   an event that permanently failed to be consumed after MassTransit's retry pipeline
//   (cfg.UseMessageRetry) exhausted every attempt and DeadLetterFaultObserver
//   (Infrastructure layer, an IConsumeObserver) recorded the fault. Immutable, contains no
//   domain logic.
// SchemaVersion: 1

using System;

namespace PlantSafety.Application.Messaging.Models;

/// <summary>A single dead-lettered event, as recorded by the MassTransit fault pipeline.</summary>
/// <param name="DeadLetterId">Primary key of the dead-letter row.</param>
/// <param name="MessageId">The originating outbox <c>MessageId</c>, if the faulted message originated from the outbox.</param>
/// <param name="EventType">Canonical event-type discriminator (e.g. <c>"RiskManagementCreated"</c>) or the CLR message type name if unresolved.</param>
/// <param name="SchemaVersion">Event envelope schema edition (e.g. <c>"2026.1"</c>).</param>
/// <param name="TenantId">Tenant the faulted message was scoped to, if known.</param>
/// <param name="Payload">Serialized JSON payload of the faulted message, if serialization succeeded.</param>
/// <param name="FaultReason">Sanitized, human-readable summary of why the message faulted.</param>
/// <param name="ExceptionDetail">Full exception detail captured from MassTransit's <c>Fault&lt;T&gt;</c>. SIEM-safe: contains exception type/message/stack, no PII/secrets.</param>
/// <param name="RetryCount">Number of delivery attempts recorded before the message was dead-lettered.</param>
/// <param name="FaultedAtUtc">UTC instant the final (dead-lettering) fault occurred.</param>
/// <param name="CreatedAtUtc">UTC instant the dead-letter row was written.</param>
/// <param name="CorrelationId">Propagated correlation identifier for cross-flow tracing.</param>
/// <param name="CausationId">Propagated causation identifier identifying the immediate parent operation.</param>
/// <param name="ActorId">Actor identifier recovered from the outbox envelope headers, if present.</param>
/// <param name="ConsumerName">Best-effort identifier of the receive endpoint/consumer that faulted.</param>
/// <param name="ReplayedAtUtc">UTC instant the event was successfully replayed, or <see langword="null"/> if not yet replayed.</param>
/// <param name="ReplayedByUserId">The user who triggered the replay, or <see langword="null"/> if not yet replayed.</param>
public sealed record DeadLetterEventDto(
    Guid DeadLetterId,
    Guid? MessageId,
    string EventType,
    string SchemaVersion,
    string? TenantId,
    string? Payload,
    string FaultReason,
    string? ExceptionDetail,
    int RetryCount,
    DateTime FaultedAtUtc,
    DateTime CreatedAtUtc,
    Guid? CorrelationId,
    Guid? CausationId,
    string? ActorId,
    string? ConsumerName,
    DateTime? ReplayedAtUtc,
    Guid? ReplayedByUserId)
{
    /// <summary>Gets a value indicating whether this event has already been successfully replayed.</summary>
    public bool IsReplayed => ReplayedAtUtc is not null;
}
