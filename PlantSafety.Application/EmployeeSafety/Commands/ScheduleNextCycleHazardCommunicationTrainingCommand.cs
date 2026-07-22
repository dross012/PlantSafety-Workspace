// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to ScheduleNextCycle (lifecycle transition) for HazardCommunicationTraining ("EmployeeSafety.HazardCommunicationTraining").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Invokes <c>HazardCommunicationTraining.ScheduleNextCycle</c> on an existing HazardCommunicationTraining aggregate.</summary>
public sealed record ScheduleNextCycleHazardCommunicationTrainingCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid HazardCommunicationTrainingId,
    DateTime DueAtUtc,
    Guid PerformedByUserId) : ICommand<Result<HazardCommunicationTrainingActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<HazardCommunicationTrainingActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
