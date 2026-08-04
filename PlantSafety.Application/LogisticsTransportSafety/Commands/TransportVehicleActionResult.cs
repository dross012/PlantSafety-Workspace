// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for TransportVehicle lifecycle commands ("LogisticsTransportSafety.TransportVehicles").
// SchemaVersion: 1

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Result of a successful TransportVehicle lifecycle command execution.</summary>
/// <param name="TransportVehicleId">Identity of the affected TransportVehicle aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record TransportVehicleActionResult(
    Guid TransportVehicleId,
    string Status);
