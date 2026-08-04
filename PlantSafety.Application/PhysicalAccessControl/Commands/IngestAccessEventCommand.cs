// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to ingest a raw badge-system event ("PhysicalAccessControl.AccessEvents").
//   Invoked by the Infrastructure.Integrations webhook/IoT ingestion pipeline.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.Commands;

/// <summary>Ingests a single raw physical-access event from an external badge system.</summary>
public sealed record IngestAccessEventCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    Guid AccessPointId,
    Guid? CredentialId,
    Guid? SafetyZoneId,
    string EventType,
    string Direction,
    DateTime OccurredAtUtc,
    Guid PerformedByUserId) : ICommand<Result<AccessEventActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<AccessEventActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command (system actor for automated ingestion).</summary>
    public Guid ActorId => PerformedByUserId;
}
