// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for EnvironmentalHealthMonitoring lifecycle commands ("EmployeeSafety.EnvironmentalHealthMonitoring").
// SchemaVersion: 1

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Result of a successful EnvironmentalHealthMonitoring lifecycle command execution.</summary>
/// <param name="EnvironmentalHealthMonitoringId">Identity of the affected EnvironmentalHealthMonitoring aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record EnvironmentalHealthMonitoringActionResult(
    Guid EnvironmentalHealthMonitoringId,
    string Status);