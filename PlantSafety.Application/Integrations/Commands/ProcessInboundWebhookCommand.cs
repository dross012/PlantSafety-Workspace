// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Command to process an inbound webhook event received from an external system.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Integrations.Commands;

/// <summary>
/// Processes a verified inbound webhook event, deferring actual downstream side effects to the
/// outbox pattern in the Infrastructure layer. Idempotent — callers may retry safely using the
/// same <paramref name="EventId"/>.
/// </summary>
/// <param name="EventId">Sender-supplied unique event identifier, used for idempotency.</param>
/// <param name="EventType">Discriminator identifying the type of event.</param>
/// <param name="Source">Identifier of the external source system.</param>
/// <param name="TenantId">The PlantSafety tenant this event belongs to.</param>
/// <param name="SiteId">Optional site scope within the tenant.</param>
/// <param name="OccurredAt">UTC timestamp when the event occurred in the source system.</param>
/// <param name="RawPayload">The raw JSON body of the webhook request, for downstream deserialization.</param>
public sealed record ProcessInboundWebhookCommand(
    string EventId,
    string EventType,
    string Source,
    Guid TenantId,
    Guid? SiteId,
    DateTimeOffset OccurredAt,
    string RawPayload) : ICommand<Result<ProcessInboundWebhookResult>>
{
    /// <summary>Gets the unique identifier of this command instance (UUIDv7).</summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();

    /// <summary>Gets the correlation identifier that groups all operations in a single business transaction.</summary>
    public Guid CorrelationId { get; init; } = Guid.NewGuid();

    /// <summary>Gets the causation identifier that identifies the immediate parent command or event.</summary>
    public Guid? CausationId { get; init; }

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.
    /// Placeholder: inbound webhooks originate from external systems, not a PlantSafety user actor; defaults to <see cref="Guid.Empty"/>.</summary>
    public Guid ActorId { get; init; } = Guid.Empty;
}
