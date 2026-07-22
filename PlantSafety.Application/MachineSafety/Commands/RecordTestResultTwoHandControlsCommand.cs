// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to RecordTestResult (lifecycle transition) for TwoHandControls ("MachineSafety.TwoHandControls").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.MachineSafety.Commands;

/// <summary>Invokes <c>TwoHandControls.RecordTestResult</c> on an existing TwoHandControls aggregate.</summary>
public sealed record RecordTestResultTwoHandControlsCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid TwoHandControlsId,
    bool Synchronized,
    decimal SynchronizationScore,
    Guid TestedByUserId,
    Guid PerformedByUserId) : ICommand<Result<TwoHandControlsActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<TwoHandControlsActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
