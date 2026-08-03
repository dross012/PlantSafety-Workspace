// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           DetectedAnomaly.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Value object recording a single cross-site/systemic anomaly detected by
//                  IGlobalSafetyAnomalyDetector. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Enums;

namespace PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;

/// <summary>A single cross-site/systemic anomaly detected within an analysis window.</summary>
public sealed record DetectedAnomaly
{
    /// <summary>Category of the anomaly.</summary>
    public required GlobalSafetyAnomalyKind Kind { get; init; }

    /// <summary>Site the anomaly pertains to, or <see langword="null"/> when the anomaly spans an entire region/machine family.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Machine family the anomaly pertains to, when <see cref="Kind"/> is <see cref="GlobalSafetyAnomalyKind.SystemicMachineLineFailure"/>.</summary>
    public string? MachineFamily { get; init; }

    /// <summary>
    /// Statistical magnitude of the deviation: a z-score (standard deviations from the cross-site
    /// mean) for count-based anomalies, or a deviation ratio (observed/baseline) for drift-based
    /// anomalies. See GlobalSafetyAnomalyDetector remarks for which applies per <see cref="Kind"/>.
    /// </summary>
    public required double DeviationMagnitude { get; init; }

    /// <summary>Human-readable explanation of the anomaly, including the raw counts/threshold that triggered it.</summary>
    public required string Message { get; init; }

    /// <summary>UTC instant the anomaly was detected.</summary>
    public required UtcTimestamp DetectedAtUtc { get; init; }
}
