// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to resolve the underlying hazard/condition behind an Alert.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Alerts.Commands;

/// <summary>Resolves the underlying hazard/condition behind an Alert.</summary>
/// <param name="CorrelationId">Correlation lineage identifier for this operation group.</param>
/// <param name="CausationId">Identifier of the immediate parent command/event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="AlertId">Identifier of the alert to resolve.</param>
/// <param name="ResponseActionsTaken">Summary of response actions taken to resolve the hazard.</param>
/// <param name="ResolvedByUserId">Identity of the user resolving the alert.</param>
public sealed record ResolveAlertCommand(
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    Guid AlertId,
    string ResponseActionsTaken,
    Guid ResolvedByUserId) : ICommand<Result<ResolveAlertResult>>
{
    /// <summary>Gets the unique identifier of this command instance.</summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";
    /// <summary>Placeholder: no tenant field exists on this command; defaults to <see cref="Guid.Empty"/> until tenant scoping is added.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <summary>Maps to <see cref="ResolvedByUserId"/>.</summary>
    public Guid ActorId => ResolvedByUserId;
}

/// <summary>Result of <see cref="ResolveAlertCommand"/>.</summary>
public sealed record ResolveAlertResult(Guid AlertId, string Status);
