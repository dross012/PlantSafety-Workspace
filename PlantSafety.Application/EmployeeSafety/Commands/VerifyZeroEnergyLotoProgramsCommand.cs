// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to VerifyZeroEnergy (lifecycle transition) for LotoPrograms ("EmployeeSafety.LotoPrograms").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Invokes <c>LotoPrograms.VerifyZeroEnergy</c> on an existing LotoPrograms aggregate.</summary>
public sealed record VerifyZeroEnergyLotoProgramsCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid LotoProgramsId,
    Guid VerifiedByUserId,
    Guid PerformedByUserId) : ICommand<Result<LotoProgramsActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<LotoProgramsActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
