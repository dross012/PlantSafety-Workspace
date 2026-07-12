// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Result payload for ProcessInboundWebhookCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Integrations.Commands;

/// <summary>Result of <see cref="ProcessInboundWebhookCommand"/>.</summary>
/// <param name="EventId">The event identifier that was processed.</param>
/// <param name="Accepted">Whether the event was accepted for downstream processing.</param>
public sealed record ProcessInboundWebhookResult(
    string EventId,
    bool Accepted);
