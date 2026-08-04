// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to RecordInspectionResult (lifecycle transition) for TransportVehicle ("LogisticsTransportSafety.TransportVehicles").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Invokes <c>TransportVehicle.RecordInspectionResult</c> on an existing TransportVehicle aggregate.</summary>
public sealed record RecordInspectionResultTransportVehicleCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid TransportVehicleId,
    bool Passed,
    decimal InspectionScore,
    Guid PerformedByUserId) : ICommand<Result<TransportVehicleActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<TransportVehicleActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
