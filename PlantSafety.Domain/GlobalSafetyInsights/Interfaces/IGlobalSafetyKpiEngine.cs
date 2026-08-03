// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           IGlobalSafetyKpiEngine.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Domain service contract computing cross-site TRIR/LTIR/exposure-rate/incident-
//                  density/machine-state-correlation KPIs from a normalized telemetry set. Placed in
//                  Domain (not Application) mirroring Analytics.Dashboard.Kpi.Services'
//                  OeeCalculator/MttrCalculator placement convention -- pure computation, no I/O.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;

namespace PlantSafety.Domain.GlobalSafetyInsights.Interfaces;

/// <summary>Computes cross-site safety KPIs from a set of normalized telemetry records.</summary>
public interface IGlobalSafetyKpiEngine
{
    /// <summary>Computes the full cross-site KPI snapshot for the given normalized telemetry set.</summary>
    GlobalSafetyKpiSnapshot Compute(IReadOnlyList<SafetyTelemetryEvent> telemetry);
}
