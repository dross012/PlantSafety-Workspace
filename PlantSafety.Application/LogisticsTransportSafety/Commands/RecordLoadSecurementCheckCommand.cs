// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to record (create) a new LoadSecurementCheck ("LogisticsTransportSafety.LoadSecurementChecks").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Records the outcome of a load securement check performed prior to or during transport.</summary>
public sealed record RecordLoadSecurementCheckCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    Guid? TripId,
    Guid VehicleId,
    Guid CheckedByUserId,
    bool Passed,
    int DeficiencyCount,
    decimal SecurementScore,
    Guid PerformedByUserId) : ICommand<Result<LoadSecurementCheckActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<LoadSecurementCheckActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
