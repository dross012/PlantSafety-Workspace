// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Application
//  File:           GetPredictiveAnalyticsByIdQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single PredictiveAnalytics read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Distinct from the pre-existing GetPredictiveAnalyticsQuery
//                  (date-range list query). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.PredictiveAnalytics.Models;

namespace PlantSafety.Application.Analytics.PredictiveAnalytics.Queries;

/// <summary>Retrieves a single PredictiveAnalytics read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="PredictiveAnalyticsId">Identity of the PredictiveAnalytics record to retrieve.</param>
public sealed record GetPredictiveAnalyticsByIdQuery(
    Guid TenantId,
    Guid PredictiveAnalyticsId) : IRequest<Result<PredictiveAnalyticsItemDto>>;
