// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           IGlobalSafetyRiskScorer.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Domain service contract producing per-site/per-region/per-machine-family risk
//                  scores by composing RiskManagement's REAL Severity x Likelihood
//                  RiskManagement.ValueObjects.RiskScore -- never reinventing risk-scoring math.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;
using PlantSafety.Domain.RiskManagement.Aggregates;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;

namespace PlantSafety.Domain.GlobalSafetyInsights.Interfaces;

/// <summary>Aggregates real RiskManagement Risk records into per-site/region/machine-family scores.</summary>
public interface IGlobalSafetyRiskScorer
{
    /// <summary>
    /// Aggregates the given tenant's scored Risk records (Risk.CurrentScore) into one
    /// <see cref="SiteRiskScore"/> per site (grouped by <see cref="Risk.SiteId"/>) plus one
    /// region-wide roll-up.
    /// </summary>
    IReadOnlyList<SiteRiskScore> ScoreBySite(IReadOnlyList<Risk> risks, Region region);
}
