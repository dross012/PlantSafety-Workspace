// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for DischargeMonitoringReport lifecycle commands ("EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport").
// SchemaVersion: 1

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Result of a successful DischargeMonitoringReport lifecycle command execution.</summary>
/// <param name="DischargeMonitoringReportId">Identity of the affected DischargeMonitoringReport aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record DischargeMonitoringReportActionResult(
    Guid DischargeMonitoringReportId,
    string Status);