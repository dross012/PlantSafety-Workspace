// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           ListElevatedCognitiveRiskPredictionsQuery.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Query listing all Elevated/Critical, non-dismissed cognitive-risk predictions
//                  for a tenant/site. Backs the Predictive Cognitive Risk Board dashboard and the
//                  RiskManagement/AccidentManagement risk-hook correlation feed.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Queries;

/// <summary>Lists all Elevated/Critical, non-dismissed cognitive-risk predictions for a tenant/site.</summary>
public sealed record ListElevatedCognitiveRiskPredictionsQuery(
    Guid TenantId,
    Guid? SiteId) : IRequest<Result<IReadOnlyList<CognitiveRiskPredictionDto>>>;
