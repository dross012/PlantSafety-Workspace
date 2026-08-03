// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyAnomalyKind.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Simple enum (Handbook §2.7.B) categorizing the kind of cross-site anomaly
//                  detected by IGlobalSafetyAnomalyDetector. Simple (not Smart) because each
//                  category carries no additional per-value metadata/behavior beyond its name --
//                  every threshold/statistical parameter lives on the detector service itself.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.GlobalSafetyInsights.Enums;

/// <summary>Category of a cross-site/systemic anomaly detected across the site network.</summary>
public enum GlobalSafetyAnomalyKind
{
    /// <summary>A single site's incident count deviates significantly (z-score) from the cross-site mean.</summary>
    CrossSiteIncidentSpike = 0,

    /// <summary>The same machine family exhibits degraded state/anomaly readings across multiple sites concurrently.</summary>
    SystemicMachineLineFailure = 1,

    /// <summary>A site/region shows a recurring pattern of worker-exposure exceedances across the analysis window.</summary>
    RecurringExposureExceedance = 2,

    /// <summary>A site's aggregated risk score has drifted significantly upward relative to its own trailing baseline.</summary>
    RiskScoreDrift = 3,
}
