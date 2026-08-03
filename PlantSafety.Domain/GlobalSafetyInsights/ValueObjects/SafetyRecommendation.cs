// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           SafetyRecommendation.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Structured (not free-text-only) recommendation record surfaced alongside a
//                  computed GlobalSafetyInsightsAggregate snapshot -- derived deterministically
//                  from the detected anomalies/risk scores by GlobalSafetyRiskScorer/
//                  GlobalSafetyAnomalyDetector at compute time. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Enums;

namespace PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;

/// <summary>A single structured, actionable safety recommendation.</summary>
public sealed record SafetyRecommendation
{
    /// <summary>Urgency of this recommendation.</summary>
    public required GlobalSafetyRecommendationPriority Priority { get; init; }

    /// <summary>Short machine-readable category (e.g. "RiskMitigation", "ExposureControl", "MachineMaintenance").</summary>
    public required string Category { get; init; }

    /// <summary>Human-readable recommendation description.</summary>
    public required string Description { get; init; }

    /// <summary>Site this recommendation targets, or <see langword="null"/> when it targets the whole region/network.</summary>
    public Guid? TargetSiteId { get; init; }

    /// <summary>Kind of anomaly this recommendation was derived from, if any.</summary>
    public GlobalSafetyAnomalyKind? RelatedAnomalyKind { get; init; }
}
