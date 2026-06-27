// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Wire envelope wrapping every integration event published to Azure Service Bus.
//   All Application-layer integration events are serialised inside this envelope before entering
//   the MassTransit outbox processor.  The envelope is the canonical contract between the outbox
//   processor (Infrastructure) and downstream consumers.
// SchemaVersion: 1

namespace PlantSafety.Application.Contracts.Messages;

/// <summary>
/// Wire envelope for all integration events published to Azure Service Bus via MassTransit 8.
/// Consumers must inspect <see cref="EventType"/> before deserialising <see cref="Payload"/>.
/// </summary>
public sealed record ServiceBusMessageEnvelope
{
    /// <summary>Unique envelope identifier (UUIDv7).</summary>
    public required Guid MessageId { get; init; }

    /// <summary>Canonical event type string.  Format: <c>Module.EventName.vN</c>.</summary>
    public required string EventType { get; init; }

    /// <summary>Tenant identifier for multi-tenancy routing.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Distributed trace correlation identifier.</summary>
    public required Guid CorrelationId { get; init; }

    /// <summary>Causation chain identifier (identifier of the command that caused this event).</summary>
    public required Guid CausationId { get; init; }

    /// <summary>UTC timestamp when the event occurred in the domain.</summary>
    public required DateTimeOffset OccurredAtUtc { get; init; }

    /// <summary>UTC timestamp when this envelope was created by the outbox processor.</summary>
    public required DateTimeOffset EnqueuedAtUtc { get; init; }

    /// <summary>Schema version used to serialise <see cref="Payload"/>.</summary>
    public required int SchemaVersion { get; init; }

    /// <summary>JSON-serialised event payload.  Consumers resolve the concrete type from <see cref="EventType"/>.</summary>
    public required string Payload { get; init; }

    /// <summary>Number of times this message has been dispatched (incremented by the outbox processor on retry).</summary>
    public required int DispatchAttempt { get; init; }
}
