// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to dispatch a response for an acknowledged Alert.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Alerts.Commands;

/// <summary>Records that a response has been dispatched for an acknowledged Alert.</summary>
/// <param name="CorrelationId">Correlation lineage identifier for this operation group.</param>
/// <param name="CausationId">Identifier of the immediate parent command/event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="AlertId">Identifier of the alert to dispatch a response for.</param>
/// <param name="DispatchedTo">Description of who/what was dispatched (e.g. "First Responders - Site A").</param>
/// <param name="DispatchedByUserId">Identity of the user recording the dispatch.</param>
public sealed record DispatchAlertCommand(
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    Guid AlertId,
    string DispatchedTo,
    Guid DispatchedByUserId) : ICommand<Result<DispatchAlertResult>>
{
    /// <summary>Gets the unique identifier of this command instance.</summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";
    /// <summary>Placeholder: no tenant field exists on this command; defaults to <see cref="Guid.Empty"/> until tenant scoping is added.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <summary>Maps to <see cref="DispatchedByUserId"/>.</summary>
    public Guid ActorId => DispatchedByUserId;
}

/// <summary>Result of <see cref="DispatchAlertCommand"/>.</summary>
public sealed record DispatchAlertResult(Guid AlertId, string Status);
