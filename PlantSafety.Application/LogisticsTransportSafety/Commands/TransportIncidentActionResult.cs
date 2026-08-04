// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for TransportIncident lifecycle commands ("LogisticsTransportSafety.TransportIncidents").
// SchemaVersion: 1

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Result of a successful TransportIncident lifecycle command execution.</summary>
/// <param name="TransportIncidentId">Identity of the affected TransportIncident aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record TransportIncidentActionResult(
    Guid TransportIncidentId,
    string Status);
