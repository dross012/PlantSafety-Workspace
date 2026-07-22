-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Schedules
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Schedules_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[SchedulesReadModel],
--                  backing ISchedulesRepository.GetSchedulesAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_Schedules_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ScheduleId],
        [TenantId],
        [SiteId],
        [ShiftName],
        [ScheduleType],
        [Status],
        [ScheduledAtUtc],
        [AssignedBy]
FROM [Operational].[SchedulesReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScheduledAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScheduledAtUtc] DESC;
GO
