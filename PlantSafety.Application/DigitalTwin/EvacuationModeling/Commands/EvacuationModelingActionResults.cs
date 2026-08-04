// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payloads for EvacuationModeling lifecycle commands
//   ("DigitalTwin.EvacuationModeling").
// SchemaVersion: 1

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;

/// <summary>Result of a successful EvacuationRoute lifecycle command execution.</summary>
public sealed record EvacuationRouteActionResult(Guid EvacuationRouteId, string Status);

/// <summary>Result of a successful EvacuationZone lifecycle command execution.</summary>
public sealed record EvacuationZoneActionResult(Guid EvacuationZoneId, string Status);

/// <summary>Result of a successful EgressSimulationRun lifecycle command execution.</summary>
public sealed record EgressSimulationActionResult(Guid EgressSimulationRunId, string Status, int? SimulatedEvacuationTimeSeconds);
