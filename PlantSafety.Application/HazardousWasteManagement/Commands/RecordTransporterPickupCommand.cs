// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to record transporter pickup/custody transfer against a manifest ("HazardousWasteManagement.Manifests").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HazardousWasteManagement.Commands;

/// <summary>Invokes <c>HazardousWasteManifest.RecordTransporterPickup</c>. From this point the manifest becomes immutable.</summary>
public sealed record RecordTransporterPickupCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid HazardousWasteManifestId,
    Guid TransporterId,
    Guid PerformedByUserId) : ICommand<Result<HazardousWasteManifestActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<HazardousWasteManifestActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
