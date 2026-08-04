// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to CloseResponse (lifecycle transition) for LimitExceedance ("EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Invokes <c>LimitExceedance.CloseResponse</c> on an existing LimitExceedance aggregate.</summary>
public sealed record CloseExceedanceResponseCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid LimitExceedanceId,
    Guid ResponseUserId,
    string? ResolutionNotes,
    Guid PerformedByUserId) : ICommand<Result<LimitExceedanceActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<LimitExceedanceActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}