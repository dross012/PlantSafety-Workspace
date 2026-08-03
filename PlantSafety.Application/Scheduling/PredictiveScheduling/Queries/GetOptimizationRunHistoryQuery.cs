// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Application
//  File:           GetOptimizationRunHistoryQuery.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve the OptimizationRun history for a tenant/site scope and date
//                  range, optionally filtered by status, most-recent first.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.PredictiveScheduling.Models;

namespace PlantSafety.Application.Scheduling.PredictiveScheduling.Queries;

/// <summary>Retrieves the OptimizationRun history for a tenant/site scope and date range.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null returns data for all accessible sites.</param>
/// <param name="FromUtc">Inclusive start of the request date range (UTC). Defaults to 30 days before now.</param>
/// <param name="ToUtc">Inclusive end of the request date range (UTC). Defaults to now.</param>
/// <param name="StatusCode">Optional <see cref="Domain.Scheduling.PredictiveScheduling.Enums.OptimizationRunStatus"/> filter.</param>
public sealed record GetOptimizationRunHistoryQuery(
    Guid TenantId,
    Guid? SiteId,
    DateTime? FromUtc,
    DateTime? ToUtc,
    string? StatusCode) : IRequest<Result<IReadOnlyList<OptimizationRunSummaryDto>>>;
