-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PlatformHealthDashboard_GetSummary.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code performance-optimization-agent)
--  SchemaVersion:  2026.1
--  Description:    Aggregates [PlatformHealth].[RequestMetricEvent] into one row per business
--                  hour: request throughput, P50/P95/P99 latency (PERCENTILE_CONT), and error
--                  rate (StatusCode >= 500), for the tenant within [@FromUtc, @ToUtc]. Backs
--                  GetPlatformHealthDashboardQuery. Additive-only evolution.
--  UPDATED:        2026-08-03 (Claude Code performance-optimization-agent, Phase 9 §2
--                  Performance & Load Profiling pass) -- the original shape ran three
--                  CROSS APPLYs directly against the ungrouped Buckets CTE, each re-filtered
--                  by "WHERE B2.BusinessHour = B.BusinessHour" and re-evaluated once per
--                  individual request-event row in B (not once per hour bucket). For H
--                  distinct hours with N requests/hour that is O(H * N^2) sort/window work
--                  (three percentile sorts recomputed per row) before the final GROUP BY even
--                  collapses the result down to one row per hour -- a genuine hot-path cost
--                  under real request volume. Rewritten so PERCENTILE_CONT is evaluated exactly
--                  once per row via a single windowed pass over Buckets (Percentiles CTE,
--                  DISTINCT-collapsed to one row per BusinessHour) and joined to the
--                  independently-aggregated per-hour counts (Counts CTE). Same output shape/
--                  columns; O(N log N) instead of O(H * N^2).
-- ============================================================================

IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'usp_PlatformHealthDashboard_GetSummary' AND schema_id = SCHEMA_ID('PlatformHealth'))
BEGIN
    DROP PROCEDURE [PlatformHealth].[usp_PlatformHealthDashboard_GetSummary];
END
GO

CREATE PROCEDURE [PlatformHealth].[usp_PlatformHealthDashboard_GetSummary]
    @TenantId UNIQUEIDENTIFIER,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Buckets AS (
        SELECT
            DATEADD(HOUR, DATEDIFF(HOUR, 0, [OccurredAtUtc]), 0) AS BusinessHour,
            [ElapsedMs],
            [StatusCode]
        FROM [PlatformHealth].[RequestMetricEvent]
        WHERE [TenantId] = @TenantId
          AND [OccurredAtUtc] >= @FromUtc
          AND [OccurredAtUtc] <  @ToUtc
    ),
    Percentiles AS (
        SELECT DISTINCT
            BusinessHour,
            PERCENTILE_CONT(0.50) WITHIN GROUP (ORDER BY ElapsedMs) OVER (PARTITION BY BusinessHour) AS P50LatencyMs,
            PERCENTILE_CONT(0.95) WITHIN GROUP (ORDER BY ElapsedMs) OVER (PARTITION BY BusinessHour) AS P95LatencyMs,
            PERCENTILE_CONT(0.99) WITHIN GROUP (ORDER BY ElapsedMs) OVER (PARTITION BY BusinessHour) AS P99LatencyMs
        FROM Buckets
    ),
    Counts AS (
        SELECT
            BusinessHour,
            COUNT(*) AS TotalRequests,
            SUM(CASE WHEN StatusCode >= 500 THEN 1 ELSE 0 END) AS ErrorCount,
            CAST(SUM(CASE WHEN StatusCode >= 500 THEN 1.0 ELSE 0.0 END) / NULLIF(COUNT(*), 0) AS FLOAT) AS ErrorRate
        FROM Buckets
        GROUP BY BusinessHour
    )
    SELECT
        C.BusinessHour,
        C.TotalRequests,
        C.ErrorCount,
        C.ErrorRate,
        P.P50LatencyMs,
        P.P95LatencyMs,
        P.P99LatencyMs
    FROM Counts C
    JOIN Percentiles P ON P.BusinessHour = C.BusinessHour
    ORDER BY C.BusinessHour;
END
GO
