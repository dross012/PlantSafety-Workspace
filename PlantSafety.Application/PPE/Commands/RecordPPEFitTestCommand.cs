// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to record a respirator/PPE fit test result for a worker's assignment
// ("PPE Training & Fit Testing").
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PPE.Commands;

/// <summary>Records a fit test result for a PPEAssignment.</summary>
public sealed record RecordPPEFitTestCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid PPEAssignmentId,
    bool Passed,
    string? Notes,
    DateTime? FitTestExpiresAtUtc,
    Guid PerformedByUserId) : ICommand<Result<PPEAssignmentActionResult>>
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PPEAssignmentActionResult>>.CausationId => CausationId;
}
