// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyKpiSnapshot.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Immutable value object capturing the cross-site KPI outputs computed by
//                  IGlobalSafetyKpiEngine for a single SafetyAnalysisTimeWindow: TRIR, LTIR
//                  (OSHA-standard incidence-rate formulas, see GlobalSafetyKpiEngine remarks for the
//                  reused-vs-new decision), worker exposure rate, incident density (incidents per
//                  site per day), and machine-state/incident correlation. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;

/// <summary>Immutable cross-site KPI snapshot for a single analysis window.</summary>
public sealed record GlobalSafetyKpiSnapshot
{
    /// <summary>
    /// Total Recordable Incident Rate: OSHA-standard formula (Total Recordable Incidents x
    /// 200,000) / Total Hours Worked, aggregated across every site in scope.
    /// </summary>
    public required decimal Trir { get; init; }

    /// <summary>
    /// Lost Time Incident Rate: OSHA-standard formula (Lost Time Incidents x 200,000) / Total
    /// Hours Worked, aggregated across every site in scope.
    /// </summary>
    public required decimal Ltir { get; init; }

    /// <summary>
    /// Worker exposure rate: mean exposure-level score (from EmployeeSafety.EnvironmentalHealthMonitoring
    /// measurements) across every site/measurement in scope, on that module's existing 0-100+ scale.
    /// </summary>
    public required decimal ExposureRate { get; init; }

    /// <summary>Incident density: total recordable incidents per site per day across the analysis window.</summary>
    public required decimal IncidentDensity { get; init; }

    /// <summary>
    /// Pearson correlation coefficient (-1..1) between each site's degraded-machine-state
    /// observation count and its recordable-incident count over the analysis window. Positive
    /// values indicate sites with more machine degradation also report more incidents.
    /// </summary>
    public required double MachineStateIncidentCorrelation { get; init; }

    /// <summary>Total recordable incidents the KPIs above were computed from (denominator transparency/audit).</summary>
    public required int TotalRecordableIncidents { get; init; }

    /// <summary>Total lost-time incidents the KPIs above were computed from.</summary>
    public required int TotalLostTimeIncidents { get; init; }

    /// <summary>Total hours worked across every site in scope for the analysis window.</summary>
    public required decimal TotalHoursWorked { get; init; }
}
