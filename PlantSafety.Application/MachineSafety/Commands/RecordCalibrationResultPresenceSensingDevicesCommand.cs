// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to RecordCalibrationResult (lifecycle transition) for PresenceSensingDevices ("MachineSafety.PresenceSensingDevices").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.MachineSafety.Commands;

/// <summary>Invokes <c>PresenceSensingDevices.RecordCalibrationResult</c> on an existing PresenceSensingDevices aggregate.</summary>
public sealed record RecordCalibrationResultPresenceSensingDevicesCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid PresenceSensingDevicesId,
    decimal DetectionAccuracyScore,
    Guid CalibratedByUserId,
    Guid PerformedByUserId) : ICommand<Result<PresenceSensingDevicesActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<PresenceSensingDevicesActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
