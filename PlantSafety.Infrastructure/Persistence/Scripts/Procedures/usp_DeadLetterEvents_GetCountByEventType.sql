-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_DeadLetterEvents_GetCountByEventType.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Performance Optimization Agent, Phase 5 §7.2)
--  Description:    Simple dead-letter analytics query: count and oldest/newest age (in hours)
--                  of dead-lettered events, grouped by EventType, optionally scoped to a
--                  TenantId and a lookback window. Mirrors usp_ProcessedEvents_Purge's plain
--                  parameterised-procedure style (no dynamic SQL, @NowUtc override for testability).
-- ============================================================================

CREATE OR ALTER PROCEDURE [System].[usp_DeadLetterEvents_GetCountByEventType]
    @TenantId       NVARCHAR(100) = NULL,
    @LookbackDays   INT           = 7,
    @NowUtc         DATETIME2(3)  = NULL
AS
SET NOCOUNT ON;

IF @NowUtc IS NULL SET @NowUtc = SYSUTCDATETIME();

SELECT
    EventType,
    COUNT(*)                                                    AS DeadLetterCount,
    MIN(FaultedAt)                                              AS OldestFaultedAt,
    MAX(FaultedAt)                                              AS NewestFaultedAt,
    DATEDIFF(HOUR, MIN(FaultedAt), @NowUtc)                     AS OldestAgeHours,
    AVG(CAST(RetryCount AS FLOAT))                              AS AvgRetryCount
FROM [System].[DeadLetterEvents]
WHERE FaultedAt >= DATEADD(DAY, -@LookbackDays, @NowUtc)
  AND (@TenantId IS NULL OR TenantId = @TenantId)
GROUP BY EventType
ORDER BY DeadLetterCount DESC;
GO
