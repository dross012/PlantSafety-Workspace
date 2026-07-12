// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Repository abstraction for recording inbound webhook events for idempotent processing.
// SchemaVersion: 1

namespace PlantSafety.Application.Integrations.Commands;

/// <summary>
/// Persists inbound webhook events for idempotent, outbox-based downstream processing.
/// Implemented in the Infrastructure layer.
/// </summary>
public interface IInboundWebhookRepository
{
    /// <summary>
    /// Records a verified inbound webhook event, performing a check-and-insert against a
    /// ProcessedEvents-style table keyed by <paramref name="eventId"/> so that retried deliveries
    /// of the same event are treated idempotently, matching this solution's established
    /// idempotent-consumer convention. Downstream side effects are deferred to the outbox pattern.
    /// </summary>
    Task<ProcessInboundWebhookResult> RecordEventAsync(
        string eventId,
        string eventType,
        string source,
        Guid tenantId,
        Guid? siteId,
        DateTimeOffset occurredAt,
        string rawPayload,
        CancellationToken cancellationToken);
}
