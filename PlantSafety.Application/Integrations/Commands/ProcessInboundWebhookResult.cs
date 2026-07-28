// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Result payload for ProcessInboundWebhookCommand.
// SchemaVersion: 1
// UPDATED: 2026-07-28 -- added ETag, projected from the new
//   [Integrations].[InboundWebhookEvents].RowVersion column by
//   usp_InboundWebhookEvents_RecordEvent. Schema-only addition: no conflict-rejection logic is
//   introduced here, and WebhookReceiverV1Controller does not yet surface it in the HTTP
//   response (matches the schema-only precedent set by RiskDapperRepository.RiskRow.ETag).

namespace PlantSafety.Application.Integrations.Commands;

/// <summary>Result of <see cref="ProcessInboundWebhookCommand"/>.</summary>
/// <param name="EventId">The event identifier that was processed.</param>
/// <param name="Accepted">Whether the event was accepted for downstream processing.</param>
/// <param name="ETag">
/// The current ETag (derived from the row's RowVersion) of the recorded event, stable across
/// idempotent retries and present for both the newly-inserted and already-recorded branches.
/// </param>
public sealed record ProcessInboundWebhookResult(
    string EventId,
    bool Accepted,
    string? ETag = null);
