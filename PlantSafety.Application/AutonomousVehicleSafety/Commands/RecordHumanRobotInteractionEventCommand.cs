// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to record a new HumanRobotInteractionEvent ("AutonomousVehicleSafety.HumanRobotInteractionEvent").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AutonomousVehicleSafety.Commands;

/// <summary>Records a new human-robot interaction event (proximity alert, near miss, physical contact, or e-stop trigger).</summary>
public sealed record RecordHumanRobotInteractionEventCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    Guid AutonomousVehicleAssetId,
    string InteractionType,
    string SeverityLevel,
    decimal? DistanceMeters,
    string LocationDescription,
    Guid PerformedByUserId) : ICommand<Result<HumanRobotInteractionEventActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<HumanRobotInteractionEventActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
