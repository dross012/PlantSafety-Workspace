-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Webinars
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Webinars_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[WebinarsReadModel],
--                  backing IWebinarsRepository.GetWebinarsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_Webinars_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [WebinarId],
        [TenantId],
        [SiteId],
        [Title],
        [HostId],
        [Status],
        [ScheduledAtUtc]
FROM [Operational].[WebinarsReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScheduledAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScheduledAtUtc] DESC;
GO
