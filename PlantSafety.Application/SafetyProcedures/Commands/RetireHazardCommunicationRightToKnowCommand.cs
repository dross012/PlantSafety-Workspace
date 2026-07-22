// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to Retire (lifecycle transition) for HazardCommunicationRightToKnow ("SafetyProcedures.HazardCommunicationRightToKnow").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Invokes <c>HazardCommunicationRightToKnow.Retire</c> on an existing HazardCommunicationRightToKnow aggregate.</summary>
public sealed record RetireHazardCommunicationRightToKnowCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid HazardCommunicationRightToKnowId,
    Guid PerformedByUserId) : ICommand<Result<HazardCommunicationRightToKnowActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<HazardCommunicationRightToKnowActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
