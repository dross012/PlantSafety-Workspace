// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command to transition a SafetyProgram to a new lifecycle status.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyPrograms.Commands;

/// <summary>Transitions a SafetyProgram to a new lifecycle status.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SafetyProgramId">Identity of the program to transition.</param>
/// <param name="NewStatus">The target status (Planned/Active/OnHold/Completed).</param>
/// <param name="PerformedByUserId">Identity of the user performing the transition.</param>
public sealed record ChangeSafetyProgramStatusCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid SafetyProgramId,
    string NewStatus,
    Guid PerformedByUserId) : ICommand<Result<ChangeSafetyProgramStatusResult>>
{
    /// <inheritdoc/>
    public Guid ActorId => PerformedByUserId;
    Guid? ICommand<Result<ChangeSafetyProgramStatusResult>>.CausationId => CausationId;
}
