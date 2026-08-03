// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           GetGlobalSafetyInsightsDetailQuery.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §5 Global Safety Dashboard)
//  SchemaVersion:  2026.1
//  Description:    Query retrieving the full detail (per-scope risk scores, anomaly detail,
//                  recommendation detail) of the most recently computed GlobalSafetyInsights
//                  snapshot for a tenant -- backs the Global Safety Dashboard's anomaly-map and
//                  region-heatmap widgets. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.GlobalSafetyInsights.Models;

namespace PlantSafety.Application.GlobalSafetyInsights.Queries;

/// <summary>Retrieves the full detail of the most recently computed GlobalSafetyInsights snapshot for <paramref name="TenantId"/>.</summary>
/// <param name="TenantId">Tenant identifier resolved server-side from the operation context.</param>
public sealed record GetGlobalSafetyInsightsDetailQuery(Guid TenantId) : IRequest<Result<GlobalSafetyInsightsDetailDto?>>;
