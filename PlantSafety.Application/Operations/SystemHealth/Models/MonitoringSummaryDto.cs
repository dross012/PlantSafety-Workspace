// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.SystemHealth
//  Layer:          Application
//  File:           MonitoringSummaryDto.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Combined DB-performance / error-count summary for the System Health
//                  rollup dashboard. Composes existing Operations.Monitoring DTOs
//                  (ErrorLogItemDto, QueryPerformanceItemDto) rather than duplicating
//                  their fields -- see GetErrorLogQuery / GetQueryPerformanceQuery.
//                  Additive-only evolution.
// ============================================================================
using System.Collections.Generic;

using PlantSafety.Application.Operations.Monitoring.Models;

namespace PlantSafety.Application.Operations.SystemHealth.Models;

/// <summary>Combined DB-performance / error-count summary sourced from Operations.Monitoring.</summary>
/// <param name="RecentErrorCount">Number of [PlantSafety.ErrorDB].dbo.ErrorLog rows logged in the last 24 hours.</param>
/// <param name="HighSeverityErrorCount">Of <paramref name="RecentErrorCount"/>, how many had ErrorSeverity &gt;= 17 (severe/system per SQL Server's documented bands).</param>
/// <param name="RecentErrors">The most recent error rows (bounded, for display).</param>
/// <param name="TopQueriesByCpu">Top queries by average CPU time for the primary tenant database ("CoreDB") over the same lookback window.</param>
public sealed record MonitoringSummaryDto(
    int RecentErrorCount,
    int HighSeverityErrorCount,
    IReadOnlyList<ErrorLogItemDto> RecentErrors,
    IReadOnlyList<QueryPerformanceItemDto> TopQueriesByCpu);
