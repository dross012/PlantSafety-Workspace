// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to Close (lifecycle transition) for MachineGuardingInspections ("SafetyProcedures.MachineGuardingInspections").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Invokes <c>MachineGuardingInspections.Close</c> on an existing MachineGuardingInspections aggregate.</summary>
public sealed record CloseMachineGuardingInspectionsCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid MachineGuardingInspectionsId,
    DateTime NextDueAtUtc,
    Guid PerformedByUserId) : ICommand<Result<MachineGuardingInspectionsActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<MachineGuardingInspectionsActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
