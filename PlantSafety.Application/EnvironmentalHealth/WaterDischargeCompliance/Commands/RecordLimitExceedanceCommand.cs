// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to record a new LimitExceedance ("EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Records a new discharge limit exceedance detection.</summary>
public sealed record RecordLimitExceedanceCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    Guid DischargePointId,
    Guid EffluentSampleId,
    Guid DischargeLimitId,
    string ParameterName,
    decimal MeasuredValue,
    decimal LimitValue,
    string Severity,
    DateTime DetectedAtUtc,
    int RepeatExceedanceCount,
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