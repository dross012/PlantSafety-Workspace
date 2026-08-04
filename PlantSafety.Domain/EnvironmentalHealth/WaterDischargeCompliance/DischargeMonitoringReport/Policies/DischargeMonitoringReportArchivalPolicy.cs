// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
//  Layer:          Domain
//  File:           DischargeMonitoringReportArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for DischargeMonitoringReport aggregates. Aligned
//                  with the water discharge compliance data-governance rule that regulatory
//                  submissions and sample chain-of-custody records must be retained for a minimum
//                  of 5 years before archival is considered. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Policies;

/// <summary>Determines whether a <see cref="DischargeMonitoringReport"/> aggregate is eligible for archival.</summary>
public static class DischargeMonitoringReportArchivalPolicy
{
    /// <summary>Minimum retention period, in years, before a terminal-state record may be archived.</summary>
    public const int MinimumRetentionYears = 5;

    /// <summary>Returns <see langword="true"/> when the record is in a terminal lifecycle state and has
    /// been retained for at least <see cref="MinimumRetentionYears"/> since creation.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Aggregates.DischargeMonitoringReport aggregate, DateTime nowUtc)
        => DischargeMonitoringReportLifecycleRules.IsTerminal(aggregate.Status)
           && aggregate.CreatedAtUtc.Value.AddYears(MinimumRetentionYears) <= nowUtc;
}