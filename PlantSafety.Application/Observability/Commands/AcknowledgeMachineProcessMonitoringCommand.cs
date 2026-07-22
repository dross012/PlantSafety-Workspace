// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to acknowledge a threshold breach or anomaly flag on a machine
// process monitoring channel.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Observability.Commands;

/// <summary>Acknowledges a threshold breach/anomaly flag for a MachineProcessMonitoring channel.</summary>
public sealed record AcknowledgeMachineProcessMonitoringCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid MachineProcessMonitoringId,
    Guid AcknowledgedByUserId) : ICommand<Result<MachineProcessMonitoringActionResult>>
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => AcknowledgedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<MachineProcessMonitoringActionResult>>.CausationId => CausationId;
}
