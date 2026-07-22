// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to record a response action taken against an Alert.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Alerts.Commands;

/// <summary>Records a single response action taken against an Alert (e.g. "Lockout equipment").</summary>
/// <param name="CorrelationId">Correlation lineage identifier for this operation group.</param>
/// <param name="CausationId">Identifier of the immediate parent command/event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="AlertId">Identifier of the alert this action was taken against.</param>
/// <param name="ActionDescription">Short description of the action taken.</param>
/// <param name="Notes">Optional free-text notes.</param>
/// <param name="TakenByUserId">Identity of the user who took the action.</param>
public sealed record RecordAlertResponseActionCommand(
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    Guid AlertId,
    string ActionDescription,
    string? Notes,
    Guid TakenByUserId) : ICommand<Result<RecordAlertResponseActionResult>>
{
    /// <summary>Gets the unique identifier of this command instance.</summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";
    /// <summary>Placeholder: no tenant field exists on this command; defaults to <see cref="Guid.Empty"/> until tenant scoping is added.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <summary>Maps to <see cref="TakenByUserId"/>.</summary>
    public Guid ActorId => TakenByUserId;
}

/// <summary>Result of <see cref="RecordAlertResponseActionCommand"/>.</summary>
public sealed record RecordAlertResponseActionResult(Guid AlertId, Guid ResponseActionId);
