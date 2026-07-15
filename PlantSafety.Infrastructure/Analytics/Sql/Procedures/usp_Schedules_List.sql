-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Schedules
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Schedules_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Analytics].[Schedules],
--                  backing ISchedulesRepository.GetScheduleItemsAsync. Filters
--                  by the schedule's next-run date range.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Schedules_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ScheduleItemId],
        [TenantId],
        [SiteId],
        [ScheduleName],
        [Frequency],
        [NextRunAtUtc],
        [Status],
        [CreatedAtUtc],
        [CreatedBy]
FROM [Analytics].[Schedules]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [NextRunAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [NextRunAtUtc] ASC;
GO
