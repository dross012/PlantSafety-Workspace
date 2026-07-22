-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Seminars
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Seminars_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[SeminarsReadModel],
--                  backing ISeminarsRepository.GetSeminarsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_Seminars_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [SeminarId],
        [TenantId],
        [SiteId],
        [Title],
        [PresenterId],
        [Status],
        [ScheduledAtUtc]
FROM [Operational].[SeminarsReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScheduledAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScheduledAtUtc] DESC;
GO
