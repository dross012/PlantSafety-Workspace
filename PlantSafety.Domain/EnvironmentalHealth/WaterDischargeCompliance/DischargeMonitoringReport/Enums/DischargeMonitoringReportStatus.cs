// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
//  Layer:          Domain
//  File:           DischargeMonitoringReportStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the DischargeMonitoringReport aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Enums;

/// <summary>Lifecycle states for the <see cref="DischargeMonitoringReport"/> aggregate root.</summary>
public enum DischargeMonitoringReportStatus
{
    /// <summary>Report is being prepared and has not yet been submitted.</summary>
    Draft = 0,

    /// <summary>Report submitted to the regulator on or before its due date.</summary>
    Submitted = 1,

    /// <summary>Report submitted after its due date (reporting-on-time-rate KPI miss).</summary>
    Late = 2,

    /// <summary>Report accepted by the regulator.</summary>
    Accepted = 3,

    /// <summary>Report rejected by the regulator and requires resubmission.</summary>
    Rejected = 4,
}