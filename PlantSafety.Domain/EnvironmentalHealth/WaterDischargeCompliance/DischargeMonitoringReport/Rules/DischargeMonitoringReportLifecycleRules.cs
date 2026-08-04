// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
//  Layer:          Domain
//  File:           DischargeMonitoringReportLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the DischargeMonitoringReport
//                  aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="DischargeMonitoringReport"/> aggregate.</summary>
public static class DischargeMonitoringReportLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the report may be submitted.</summary>
    public static bool CanSubmit(DischargeMonitoringReportStatus current) => current == DischargeMonitoringReportStatus.Draft;

    /// <summary>Returns <see langword="true"/> when the report may be accepted.</summary>
    public static bool CanAccept(DischargeMonitoringReportStatus current) =>
        current is DischargeMonitoringReportStatus.Submitted or DischargeMonitoringReportStatus.Late;

    /// <summary>Returns <see langword="true"/> when the report may be rejected.</summary>
    public static bool CanReject(DischargeMonitoringReportStatus current) =>
        current is DischargeMonitoringReportStatus.Submitted or DischargeMonitoringReportStatus.Late;

    /// <summary>Returns <see langword="true"/> when <paramref name="status"/> is a terminal state.</summary>
    public static bool IsTerminal(DischargeMonitoringReportStatus status) =>
        status is DischargeMonitoringReportStatus.Accepted or DischargeMonitoringReportStatus.Rejected;
}