// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to ScheduleTest (lifecycle transition) for EmergencyStopDevices ("MachineSafety.EmergencyStopDevices").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.MachineSafety.Commands;

/// <summary>Invokes <c>EmergencyStopDevices.ScheduleTest</c> on an existing EmergencyStopDevices aggregate.</summary>
public sealed record ScheduleTestEmergencyStopDevicesCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid EmergencyStopDevicesId,
    DateTime DueAtUtc,
    Guid PerformedByUserId) : ICommand<Result<EmergencyStopDevicesActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<EmergencyStopDevicesActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
