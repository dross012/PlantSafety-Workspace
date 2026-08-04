// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to define a new evacuation route ("DigitalTwin.EvacuationModeling.EvacuationRoutes").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;

/// <summary>Defines a new <c>EvacuationRoute</c> against a validated Digital Twin model version.</summary>
public sealed record DefineEvacuationRouteCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    string RouteName,
    Guid ZoneId,
    Guid AssemblyPointId,
    string PathGeometryRef,
    string TwinModelVersion,
    int EstimatedEgressTimeSeconds,
    Guid PerformedByUserId) : ICommand<Result<EvacuationRouteActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<EvacuationRouteActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
