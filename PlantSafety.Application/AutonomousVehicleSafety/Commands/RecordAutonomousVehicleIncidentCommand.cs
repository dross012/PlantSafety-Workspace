// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to record a new AutonomousVehicleIncident ("AutonomousVehicleSafety.AutonomousVehicleIncident").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AutonomousVehicleSafety.Commands;

/// <summary>Records a new autonomous vehicle incident (collision/near-miss, geofence breach, software fault, or human-robot contact).</summary>
public sealed record RecordAutonomousVehicleIncidentCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    Guid AutonomousVehicleAssetId,
    string IncidentType,
    string SeverityLevel,
    string DescriptionSummary,
    Guid PerformedByUserId) : ICommand<Result<AutonomousVehicleIncidentActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<AutonomousVehicleIncidentActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
