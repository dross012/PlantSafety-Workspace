// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to define a new occupancy-tracked evacuation zone. Supporting
//   command not in the original named-command list but required so RecordOccupancySnapshot has a
//   zone to record against -- see module summary for this documented assumption.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;

/// <summary>Defines a new <c>EvacuationZone</c> against a validated Digital Twin model version.</summary>
public sealed record DefineEvacuationZoneCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    string ZoneName,
    string TwinModelVersion,
    int ZoneCapacity,
    int OccupancyThreshold,
    Guid PerformedByUserId) : ICommand<Result<EvacuationZoneActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<EvacuationZoneActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
