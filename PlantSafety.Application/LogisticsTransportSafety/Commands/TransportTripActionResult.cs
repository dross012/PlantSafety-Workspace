// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for TransportTrip lifecycle commands ("LogisticsTransportSafety.TransportTrips").
// SchemaVersion: 1

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Result of a successful TransportTrip lifecycle command execution.</summary>
/// <param name="TransportTripId">Identity of the affected TransportTrip aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record TransportTripActionResult(
    Guid TransportTripId,
    string Status);
