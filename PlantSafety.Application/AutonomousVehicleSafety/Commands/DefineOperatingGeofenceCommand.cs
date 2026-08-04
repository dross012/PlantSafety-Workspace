// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to define a new OperatingGeofence ("AutonomousVehicleSafety.OperatingGeofence").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AutonomousVehicleSafety.Commands;

/// <summary>Defines a new operating-domain geofence (exclusion/restricted/operating/speed-limit zone).</summary>
public sealed record DefineOperatingGeofenceCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    string GeofenceName,
    string ZoneType,
    string BoundaryDefinition,
    decimal? MaxSpeedKph,
    Guid PerformedByUserId) : ICommand<Result<OperatingGeofenceActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<OperatingGeofenceActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
