// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to dispatch an outbound incident notification to an external
// public-safety agency during a safety event.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PublicSafetyCommunication.Commands;

/// <summary>Dispatches a new IncidentNotification to an external public-safety agency.</summary>
/// <param name="IncidentId">Optional identity of the originating SafetyEvents.EmergencyManagement incident.</param>
/// <param name="PublicSafetyAgencyId">Identity of the receiving PublicSafetyAgency.</param>
/// <param name="DeliveryChannelCode">Stable code of the PublicSafetyDeliveryChannel to use.</param>
/// <param name="Message">Notification message content.</param>
/// <param name="CommandId">Unique identifier for this command instance.</param>
/// <param name="CorrelationId">Correlation identifier grouping every operation in the same business transaction.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event that caused this one.</param>
/// <param name="TenantId">Tenant the operation is scoped to.</param>
/// <param name="SiteId">Site the operation is scoped to, when site-scoped.</param>
public sealed record DispatchIncidentNotificationCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    Guid? IncidentId,
    Guid PublicSafetyAgencyId,
    string DeliveryChannelCode,
    string Message) : ICommand<Result<IncidentNotificationActionResult>>
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId { get; init; }

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<IncidentNotificationActionResult>>.CausationId => CausationId;
}
