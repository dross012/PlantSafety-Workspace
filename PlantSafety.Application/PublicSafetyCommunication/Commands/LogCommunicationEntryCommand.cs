// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to record an immutable communication log entry documenting a
// communication with an external public-safety stakeholder, for after-action review and
// compliance evidence.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PublicSafetyCommunication.Commands;

/// <summary>Records a new, immutable CommunicationLogEntry.</summary>
/// <param name="IncidentId">Optional identity of the originating SafetyEvents.EmergencyManagement incident.</param>
/// <param name="RelatedIncidentNotificationId">Optional identity of the related IncidentNotification.</param>
/// <param name="RelatedPublicAlertBroadcastId">Optional identity of the related PublicAlertBroadcast.</param>
/// <param name="Direction">Direction of the communication (Outbound/Inbound).</param>
/// <param name="Medium">Medium used for the communication.</param>
/// <param name="CounterpartyName">Name of the external counterparty (agency/contact) involved.</param>
/// <param name="Summary">Free-text summary of the communication content.</param>
/// <param name="OccurredAtUtc">UTC timestamp the communication actually occurred.</param>
public sealed record LogCommunicationEntryCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    Guid? IncidentId,
    Guid? RelatedIncidentNotificationId,
    Guid? RelatedPublicAlertBroadcastId,
    string Direction,
    string Medium,
    string CounterpartyName,
    string Summary,
    DateTime OccurredAtUtc) : ICommand<Result<CommunicationLogEntryActionResult>>
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId { get; init; }

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CommunicationLogEntryActionResult>>.CausationId => CausationId;
}
