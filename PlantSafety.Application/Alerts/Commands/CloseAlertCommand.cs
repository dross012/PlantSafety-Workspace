// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to close a resolved Alert with close-out verification.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Alerts.Commands;

/// <summary>Closes a resolved Alert, recording close-out verification per "14. Alert Logging &amp; Reporting".</summary>
/// <param name="CorrelationId">Correlation lineage identifier for this operation group.</param>
/// <param name="CausationId">Identifier of the immediate parent command/event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="AlertId">Identifier of the alert to close.</param>
/// <param name="CloseOutVerification">Close-out verification notes.</param>
/// <param name="ClosedByUserId">Identity of the user closing the alert.</param>
public sealed record CloseAlertCommand(
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    Guid AlertId,
    string CloseOutVerification,
    Guid ClosedByUserId) : ICommand<Result<CloseAlertResult>>
{
    /// <summary>Gets the unique identifier of this command instance.</summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";
    /// <summary>Placeholder: no tenant field exists on this command; defaults to <see cref="Guid.Empty"/> until tenant scoping is added.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <summary>Maps to <see cref="ClosedByUserId"/>.</summary>
    public Guid ActorId => ClosedByUserId;
}

/// <summary>Result of <see cref="CloseAlertCommand"/>.</summary>
public sealed record CloseAlertResult(Guid AlertId, string Status);
