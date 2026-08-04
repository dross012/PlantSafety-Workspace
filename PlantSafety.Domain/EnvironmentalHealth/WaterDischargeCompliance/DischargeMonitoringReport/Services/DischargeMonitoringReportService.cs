// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
//  Layer:          Domain
//  File:           DischargeMonitoringReportService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain service for DischargeMonitoringReport. Coordinates logic that cannot live on a single
//                  aggregate (e.g. cross-aggregate repeat-exceedance evaluation feeding
//                  RiskManagement escalation). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Services;

/// <summary>Domain service for DischargeMonitoringReport. Coordinates logic that cannot live on a single aggregate.</summary>
public class DischargeMonitoringReportService : IDomainService
{
    // Inject domain repositories and policies via constructor if needed.
    // Domain services must not reference infrastructure (no HTTP/SQL/logging).
}