// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to define a new DischargeLimit ("EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Defines a new discharge limit for a parameter at a discharge point.</summary>
public sealed record DefineDischargeLimitCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    Guid DischargePointId,
    Guid DischargePermitId,
    string Parameter,
    string LimitBasis,
    decimal LimitValue,
    string UnitOfMeasure,
    DateTime EffectiveAtUtc,
    string PermitConditionReference,
    Guid PerformedByUserId) : ICommand<Result<DischargeLimitActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<DischargeLimitActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}