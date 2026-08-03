// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyRiskScorer.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Real, working IGlobalSafetyRiskScorer implementation. Composes RiskManagement's
//                  REAL Severity x Likelihood RiskManagement.ValueObjects.RiskScore (Appendix B,
//                  "Risk Score = Severity x Likelihood") by averaging every scored Risk record's
//                  CurrentScore per site, then rounding back to the nearest valid 1-5 x 1-5 cell so
//                  the result remains a genuine RiskScore (never a fabricated decimal score with no
//                  banding). Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Enums;
using PlantSafety.Domain.GlobalSafetyInsights.Interfaces;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;
using PlantSafety.Domain.RiskManagement.Aggregates;
using PlantSafety.Domain.RiskManagement.ValueObjects;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;

namespace PlantSafety.Domain.GlobalSafetyInsights.Services;

/// <summary>Real, working risk scorer composing RiskManagement's Risk aggregates into per-site/region roll-ups.</summary>
public sealed class GlobalSafetyRiskScorer : IGlobalSafetyRiskScorer, IDomainService
{
    /// <inheritdoc/>
    public IReadOnlyList<SiteRiskScore> ScoreBySite(IReadOnlyList<Risk> risks, Region region)
    {
        ArgumentNullException.ThrowIfNull(risks);
        ArgumentNullException.ThrowIfNull(region);

        var scored = risks.Where(r => r.CurrentScore.HasValue).ToList();
        var results = new List<SiteRiskScore>();

        var bySite = scored
            .Where(r => r.SiteId.HasValue)
            .GroupBy(r => r.SiteId!.Value);

        foreach (var group in bySite)
        {
            results.Add(BuildScore(GlobalSafetyRiskScopeType.Site, group.Key.ToString(), group.Select(r => r.CurrentScore!.Value).ToList()));
        }

        if (scored.Count > 0)
        {
            results.Add(BuildScore(GlobalSafetyRiskScopeType.Region, region.Code, scored.Select(r => r.CurrentScore!.Value).ToList()));
        }

        return results;
    }

    private static SiteRiskScore BuildScore(GlobalSafetyRiskScopeType scopeType, string scopeKey, IReadOnlyList<RiskScore> scores)
    {
        var avgSeverity = (int)Math.Round(scores.Average(s => s.Severity), MidpointRounding.AwayFromZero);
        var avgLikelihood = (int)Math.Round(scores.Average(s => s.Likelihood), MidpointRounding.AwayFromZero);

        avgSeverity = Math.Clamp(avgSeverity, 1, 5);
        avgLikelihood = Math.Clamp(avgLikelihood, 1, 5);

        return new SiteRiskScore
        {
            ScopeType = scopeType,
            ScopeKey = scopeKey,
            AggregatedScore = new RiskScore(avgSeverity, avgLikelihood),
            SourceRiskCount = scores.Count,
        };
    }
}
