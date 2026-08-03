// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           IGlobalSafetyAnomalyDetector.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Domain service contract detecting cross-site spikes/systemic patterns/recurring
//                  machine-line issues via real statistical methods (z-score deviation from the
//                  cross-site mean), matching the statistical rigor of Phase 5's
//                  Observability.Telemetry.Services.TelemetryAnomalyDetector. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;

namespace PlantSafety.Domain.GlobalSafetyInsights.Interfaces;

/// <summary>Detects cross-site/systemic anomalies from a set of normalized telemetry records.</summary>
public interface IGlobalSafetyAnomalyDetector
{
    /// <summary>
    /// Detects cross-site incident-count spikes (per-site z-score vs. the cross-site mean),
    /// recurring exposure exceedances, and systemic machine-family failures across the given
    /// normalized telemetry set.
    /// </summary>
    IReadOnlyList<DetectedAnomaly> DetectAnomalies(IReadOnlyList<SafetyTelemetryEvent> telemetry, UtcTimestamp nowUtc);
}
