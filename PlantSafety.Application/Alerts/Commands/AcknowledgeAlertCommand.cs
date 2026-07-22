// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to acknowledge a triggered or escalated Alert.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Alerts.Commands;

/// <summary>Acknowledges an Alert, transitioning it out of the Triggered/Escalated state.</summary>
/// <param name="CorrelationId">Correlation lineage identifier for this operation group.</param>
/// <param name="CausationId">Identifier of the immediate parent command/event.</param>
/// <param name="CommandId">Unique identifier for this command instance.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="AlertId">Identifier of the alert to acknowledge.</param>
/// <param name="AcknowledgedByUserId">Identity of the user acknowledging the alert.</param>
public sealed record AcknowledgeAlertCommand(
    Guid CorrelationId,
    Guid? CausationId,
    Guid CommandId,
    DateTime RequestedAtUtc,
    Guid AlertId,
    Guid AcknowledgedByUserId) : ICommand<Result<AcknowledgeAlertResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";
    /// <summary>Placeholder: no tenant field exists on this command; defaults to <see cref="Guid.Empty"/> until tenant scoping is added.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <summary>Maps to <see cref="AcknowledgedByUserId"/>.</summary>
    public Guid ActorId => AcknowledgedByUserId;
}

/// <summary>Result of <see cref="AcknowledgeAlertCommand"/>.</summary>
/// <param name="AlertId">Identifier of the acknowledged alert.</param>
/// <param name="Status">Lifecycle status after acknowledgement.</param>
public sealed record AcknowledgeAlertResult(Guid AlertId, string Status);
