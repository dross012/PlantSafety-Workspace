// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
//  Layer:          Domain
//  File:           DischargeMonitoringReportSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for DischargeMonitoringReport aggregate rehydration. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Snapshots;

/// <summary>Immutable snapshot of a <see cref="DischargeMonitoringReport"/> aggregate for fast rehydration.</summary>
public sealed record DischargeMonitoringReportSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public DischargeMonitoringReportId DischargeMonitoringReportId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public DischargeMonitoringReportStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}