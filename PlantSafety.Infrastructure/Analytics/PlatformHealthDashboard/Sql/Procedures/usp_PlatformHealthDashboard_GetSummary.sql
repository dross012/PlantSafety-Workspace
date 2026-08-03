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
    )
    SELECT
        BusinessHour,
        COUNT(*) AS TotalRequests,
        SUM(CASE WHEN StatusCode >= 500 THEN 1 ELSE 0 END) AS ErrorCount,
        CAST(SUM(CASE WHEN StatusCode >= 500 THEN 1.0 ELSE 0.0 END) / NULLIF(COUNT(*), 0) AS FLOAT) AS ErrorRate,
        MAX(P50.Value) AS P50LatencyMs,
        MAX(P95.Value) AS P95LatencyMs,
        MAX(P99.Value) AS P99LatencyMs
    FROM Buckets B
    CROSS APPLY (SELECT DISTINCT PERCENTILE_CONT(0.50) WITHIN GROUP (ORDER BY B2.ElapsedMs) OVER (PARTITION BY B2.BusinessHour) AS Value
                 FROM Buckets B2 WHERE B2.BusinessHour = B.BusinessHour) AS P50
    CROSS APPLY (SELECT DISTINCT PERCENTILE_CONT(0.95) WITHIN GROUP (ORDER BY B3.ElapsedMs) OVER (PARTITION BY B3.BusinessHour) AS Value
                 FROM Buckets B3 WHERE B3.BusinessHour = B.BusinessHour) AS P95
    CROSS APPLY (SELECT DISTINCT PERCENTILE_CONT(0.99) WITHIN GROUP (ORDER BY B4.ElapsedMs) OVER (PARTITION BY B4.BusinessHour) AS Value
                 FROM Buckets B4 WHERE B4.BusinessHour = B.BusinessHour) AS P99
    GROUP BY BusinessHour
    ORDER BY BusinessHour;
END
GO
