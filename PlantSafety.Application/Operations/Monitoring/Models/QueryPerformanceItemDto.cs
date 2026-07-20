// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Monitoring
//  Layer:          Application
//  File:           QueryPerformanceItemDto.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    A single Query Store row summarizing recent execution statistics for
//                  one (query_id, plan_id) pair, sourced from sys.query_store_query /
//                  sys.query_store_query_text / sys.query_store_plan /
//                  sys.query_store_runtime_stats / sys.query_store_runtime_stats_interval
//                  (Query Store enabled per-database in Phase 11,
//                  111-query-store-configuration.sql). Additive-only evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.Monitoring.Models;

/// <summary>A single Query Store row summarizing recent execution statistics for one query/plan.</summary>
/// <param name="DatabaseKey">Logical database key (see MonitoringOptions.DatabaseConnectionStrings), e.g. "CoreDB".</param>
/// <param name="QueryId">sys.query_store_query.query_id.</param>
/// <param name="PlanId">sys.query_store_plan.plan_id.</param>
/// <param name="QueryText">The query's SQL text (sys.query_store_query_text.query_sql_text), truncated for display.</param>
/// <param name="ExecutionCount">Total executions across the runtime-stats intervals considered.</param>
/// <param name="AvgDurationMs">Average duration in milliseconds.</param>
/// <param name="AvgCpuTimeMs">Average CPU time in milliseconds.</param>
/// <param name="AvgLogicalReads">Average logical page reads.</param>
/// <param name="LastExecutionUtc">End time of the most recent runtime-stats interval observed for this query/plan.</param>
public sealed record QueryPerformanceItemDto(
    string DatabaseKey,
    long QueryId,
    long PlanId,
    string QueryText,
    long ExecutionCount,
    double AvgDurationMs,
    double AvgCpuTimeMs,
    double AvgLogicalReads,
    DateTime? LastExecutionUtc);
