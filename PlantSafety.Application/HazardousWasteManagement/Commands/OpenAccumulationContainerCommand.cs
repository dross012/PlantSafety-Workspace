// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to open (create) a new WasteContainer ("HazardousWasteManagement.WasteContainers").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HazardousWasteManagement.Commands;

/// <summary>Opens a new accumulation container starting the RCRA-style accumulation clock.</summary>
public sealed record OpenAccumulationContainerCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    Guid AccumulationAreaId,
    Guid WasteStreamId,
    string ContainerLabel,
    decimal CapacityGallons,
    int RegulatoryTimeLimitDays,
    Guid PerformedByUserId) : ICommand<Result<WasteContainerActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<WasteContainerActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
