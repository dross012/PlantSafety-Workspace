// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           SiteRiskScore.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Value object representing a risk score aggregated over one scope (a single
//                  site, an entire region, or a machine family across sites). Composes the REAL
//                  RiskManagement.ValueObjects.RiskScore (Severity x Likelihood, Appendix B) rather
//                  than reinventing risk-scoring math -- IGlobalSafetyRiskScorer averages/rolls up
//                  the underlying Risk aggregates' CurrentScore values per scope and wraps the
//                  result here. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Enums;
using PlantSafety.Domain.RiskManagement.Enums;
using PlantSafety.Domain.RiskManagement.ValueObjects;

namespace PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;

/// <summary>A risk score aggregated over a site, region, or machine family.</summary>
public sealed record SiteRiskScore
{
    /// <summary>Which dimension this score was aggregated over.</summary>
    public required GlobalSafetyRiskScopeType ScopeType { get; init; }

    /// <summary>
    /// Identity of the scope: the SiteId (as a string GUID) for <see cref="GlobalSafetyRiskScopeType.Site"/>,
    /// the Region <see cref="SharedKernel.Contracts.Enterprise.MultiRegion.Region.Code"/> for
    /// <see cref="GlobalSafetyRiskScopeType.Region"/>, or the machine family name for
    /// <see cref="GlobalSafetyRiskScopeType.MachineFamily"/>.
    /// </summary>
    public required string ScopeKey { get; init; }

    /// <summary>
    /// Rounded-to-nearest-integer average Severity x Likelihood score across the underlying Risk
    /// records in this scope (the real RiskManagement.ValueObjects.RiskScore banding).
    /// </summary>
    public required RiskScore AggregatedScore { get; init; }

    /// <summary>Qualitative banding of <see cref="AggregatedScore"/>, per RiskManagement's existing 5x5 matrix convention.</summary>
    public RiskLevel Level => AggregatedScore.ToRiskLevel();

    /// <summary>Number of underlying Risk records this score was averaged over.</summary>
    public required int SourceRiskCount { get; init; }
}
