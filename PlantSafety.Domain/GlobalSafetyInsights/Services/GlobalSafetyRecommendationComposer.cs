// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyRecommendationComposer.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Pure, deterministic domain service deriving structured SafetyRecommendation
//                  records from a snapshot's detected anomalies and risk scores. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Enums;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;

namespace PlantSafety.Domain.GlobalSafetyInsights.Services;

/// <summary>Derives structured, actionable recommendations from a snapshot's anomalies and risk scores.</summary>
public sealed class GlobalSafetyRecommendationComposer : IDomainService
{
    /// <summary>Composes recommendations from the given anomalies and risk scores.</summary>
    public IReadOnlyList<SafetyRecommendation> Compose(
        IReadOnlyList<DetectedAnomaly> anomalies, IReadOnlyList<SiteRiskScore> riskScores)
    {
        ArgumentNullException.ThrowIfNull(anomalies);
        ArgumentNullException.ThrowIfNull(riskScores);

        var recommendations = new List<SafetyRecommendation>();

        foreach (var anomaly in anomalies)
        {
            recommendations.Add(anomaly.Kind switch
            {
                GlobalSafetyAnomalyKind.CrossSiteIncidentSpike => new SafetyRecommendation
                {
                    Priority = GlobalSafetyRecommendationPriority.High,
                    Category = "IncidentInvestigation",
                    Description = $"Dispatch a cross-functional safety review to site {anomaly.SiteId} -- incident count is a statistical outlier vs. peer sites.",
                    TargetSiteId = anomaly.SiteId,
                    RelatedAnomalyKind = anomaly.Kind,
                },
                GlobalSafetyAnomalyKind.RecurringExposureExceedance => new SafetyRecommendation
                {
                    Priority = GlobalSafetyRecommendationPriority.High,
                    Category = "ExposureControl",
                    Description = $"Escalate industrial-hygiene controls at site {anomaly.SiteId} -- recurring exposure measurements exceed the cross-site baseline.",
                    TargetSiteId = anomaly.SiteId,
                    RelatedAnomalyKind = anomaly.Kind,
                },
                GlobalSafetyAnomalyKind.SystemicMachineLineFailure => new SafetyRecommendation
                {
                    Priority = GlobalSafetyRecommendationPriority.Critical,
                    Category = "MachineMaintenance",
                    Description = $"Initiate a cross-site maintenance bulletin for machine family '{anomaly.MachineFamily}' -- degraded state observed across multiple sites concurrently.",
                    TargetSiteId = null,
                    RelatedAnomalyKind = anomaly.Kind,
                },
                _ => new SafetyRecommendation
                {
                    Priority = GlobalSafetyRecommendationPriority.Medium,
                    Category = "RiskMitigation",
                    Description = "Review the underlying anomaly detail for a recommended remediation.",
                    TargetSiteId = anomaly.SiteId,
                    RelatedAnomalyKind = anomaly.Kind,
                },
            });
        }

        foreach (var score in riskScores.Where(s => s.Level.Code is "High" or "Critical"))
        {
            recommendations.Add(new SafetyRecommendation
            {
                Priority = score.Level.Code == "Critical" ? GlobalSafetyRecommendationPriority.Critical : GlobalSafetyRecommendationPriority.High,
                Category = "RiskMitigation",
                Description = $"{score.ScopeType} '{score.ScopeKey}' carries a {score.Level.DisplayName} aggregated risk score ({score.AggregatedScore}) across {score.SourceRiskCount} record(s) -- prioritize control-measure review.",
                TargetSiteId = score.ScopeType == Enums.GlobalSafetyRiskScopeType.Site && Guid.TryParse(score.ScopeKey, out var siteId) ? siteId : null,
                RelatedAnomalyKind = null,
            });
        }

        return recommendations;
    }
}
