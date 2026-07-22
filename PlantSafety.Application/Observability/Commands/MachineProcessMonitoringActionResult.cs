// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for MachineProcessMonitoring lifecycle commands
// (record sensor reading, raise anomaly flag, acknowledge).
// SchemaVersion: 1

namespace PlantSafety.Application.Observability.Commands;

/// <summary>Result of a successful MachineProcessMonitoring lifecycle command execution.</summary>
/// <param name="MachineProcessMonitoringId">Identity of the affected MachineProcessMonitoring aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record MachineProcessMonitoringActionResult(
    Guid MachineProcessMonitoringId,
    string Status);
