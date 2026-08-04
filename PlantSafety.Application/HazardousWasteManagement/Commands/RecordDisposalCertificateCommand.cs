// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to record (create) a new DisposalCertificate ("HazardousWasteManagement.DisposalCertificates").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HazardousWasteManagement.Commands;

/// <summary>Records a disposal certificate received from a TSDF facility, closing the chain-of-custody loop for a manifest.</summary>
public sealed record RecordDisposalCertificateCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    Guid ManifestId,
    Guid TsdfFacilityId,
    string DisposalMethod,
    DateTime DisposalDateUtc,
    Guid PerformedByUserId) : ICommand<Result<DisposalCertificateActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<DisposalCertificateActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
