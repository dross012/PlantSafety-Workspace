// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to ScheduleDrill (lifecycle transition) for EmergencyPreparedness ("EmployeeSafety.EmergencyPreparedness").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Invokes <c>EmergencyPreparedness.ScheduleDrill</c> on an existing EmergencyPreparedness aggregate.</summary>
public sealed record ScheduleDrillEmergencyPreparednessCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid EmergencyPreparednessId,
    DateTime DueAtUtc,
    Guid PerformedByUserId) : ICommand<Result<EmergencyPreparednessActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<EmergencyPreparednessActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
