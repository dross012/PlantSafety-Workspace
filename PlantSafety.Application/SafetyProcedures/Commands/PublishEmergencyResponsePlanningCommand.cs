// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to Publish (lifecycle transition) for EmergencyResponsePlanning ("SafetyProcedures.EmergencyResponsePlanning").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Invokes <c>EmergencyResponsePlanning.Publish</c> on an existing EmergencyResponsePlanning aggregate.</summary>
public sealed record PublishEmergencyResponsePlanningCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid EmergencyResponsePlanningId,
    Guid ApprovedByUserId,
    Guid PerformedByUserId) : ICommand<Result<EmergencyResponsePlanningActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<EmergencyResponsePlanningActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
