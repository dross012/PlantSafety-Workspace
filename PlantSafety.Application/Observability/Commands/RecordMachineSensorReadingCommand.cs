// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to record a new machine/process sensor reading
// ("Machine & Process Safety Monitoring" - real-time hazard alerts).
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Observability.Commands;

/// <summary>Records a new sensor reading for a MachineProcessMonitoring channel.</summary>
public sealed record RecordMachineSensorReadingCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid MachineProcessMonitoringId,
    decimal Reading,
    decimal ThresholdMin,
    decimal ThresholdMax,
    Guid PerformedByUserId) : ICommand<Result<MachineProcessMonitoringActionResult>>
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<MachineProcessMonitoringActionResult>>.CausationId => CausationId;
}
