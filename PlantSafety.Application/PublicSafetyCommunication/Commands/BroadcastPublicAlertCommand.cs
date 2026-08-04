// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to send a public alert broadcast to public warning systems
// (EAS/WEA/IPAWS, siren networks, social media) during a safety event.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PublicSafetyCommunication.Commands;

/// <summary>Sends a new PublicAlertBroadcast to a public warning system.</summary>
/// <param name="IncidentId">Optional identity of the originating SafetyEvents.EmergencyManagement incident.</param>
/// <param name="SeverityCode">Stable code of the PublicAlertSeverity (Advisory/Watch/Warning/Emergency).</param>
/// <param name="ChannelCode">Stable code of the PublicBroadcastChannel to use.</param>
/// <param name="Headline">Short headline for the alert.</param>
/// <param name="Message">Full alert message content.</param>
/// <param name="TargetArea">Description of the geographic area targeted by this broadcast.</param>
public sealed record BroadcastPublicAlertCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    Guid? IncidentId,
    string SeverityCode,
    string ChannelCode,
    string Headline,
    string Message,
    string TargetArea) : ICommand<Result<PublicAlertBroadcastActionResult>>
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId { get; init; }

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PublicAlertBroadcastActionResult>>.CausationId => CausationId;
}
