// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Segregation
//  Layer:          Application
//  File:           SuggestLoadPlanQuery.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Query that delegates to the same SegregationEngine.EvaluateSegregation call as
//                  EvaluateLoadQuery (via the shared EvaluateLoadQueryHandler.Evaluate core), but
//                  exposes only the RecommendedLoadPlan portion of the result -- distinct query
//                  name per the user's spec wording ("Suggest load plan").
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.Segregation.Models;

namespace PlantSafety.Application.Compliance.PHMSA.Segregation.Queries;

/// <summary>Suggests a recommended load plan for a proposed multi-hazmat load.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="LoadItems">The materials/hazard classes present in the proposed load.</param>
public sealed record SuggestLoadPlanQuery(
    Guid TenantId,
    IReadOnlyList<SegregationLoadItemDto> LoadItems) : IRequest<Result<SuggestLoadPlanResult>>;

/// <summary>Result of <see cref="SuggestLoadPlanQuery"/>.</summary>
/// <param name="IsCompliant">Whether the load has zero segregation violations.</param>
/// <param name="RecommendedLoadPlan">Human-readable load-plan recommendations.</param>
public sealed record SuggestLoadPlanResult(bool IsCompliant, IReadOnlyList<string> RecommendedLoadPlan);
