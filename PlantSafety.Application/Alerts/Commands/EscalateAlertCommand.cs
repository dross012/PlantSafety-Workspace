// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to escalate an Alert, manually or automatically.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Alerts.Commands;

/// <summary>Escalates an Alert due to lack of timely acknowledgement/response, or by manual override.</summary>
/// <param name="CorrelationId">Correlation lineage identifier for this operation group.</param>
/// <param name="CausationId">Identifier of the immediate parent command/event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="AlertId">Identifier of the alert to escalate.</param>
/// <param name="EscalationReason">Reason for escalation.</param>
/// <param name="WasAutoEscalated">True when escalation was triggered automatically by <c>AlertEscalationPolicy</c>.</param>
/// <param name="EscalatedByUserId">Identity of the user escalating the alert. Null for automatic escalations.</param>
public sealed record EscalateAlertCommand(
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    Guid AlertId,
    string EscalationReason,
    bool WasAutoEscalated,
    Guid? EscalatedByUserId) : ICommand<Result<EscalateAlertResult>>
{
    /// <summary>Gets the unique identifier of this command instance.</summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";
    /// <summary>Placeholder: no tenant field exists on this command; defaults to <see cref="Guid.Empty"/> until tenant scoping is added.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <summary>Maps to <see cref="EscalatedByUserId"/>; falls back to <see cref="Guid.Empty"/> for automatic escalations with no user actor.</summary>
    public Guid ActorId => EscalatedByUserId ?? Guid.Empty;
}

/// <summary>Result of <see cref="EscalateAlertCommand"/>.</summary>
public sealed record EscalateAlertResult(Guid AlertId, string Status);
