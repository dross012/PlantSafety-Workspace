// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to Acknowledge (lifecycle transition) for AccessAnomaly.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.Commands;

/// <summary>Invokes <c>AccessAnomaly.Acknowledge</c> on an existing AccessAnomaly aggregate.</summary>
public sealed record AcknowledgeAccessAnomalyCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid AccessAnomalyId,
    Guid AcknowledgedByUserId,
    Guid PerformedByUserId) : ICommand<Result<AccessAnomalyActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<AccessAnomalyActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
