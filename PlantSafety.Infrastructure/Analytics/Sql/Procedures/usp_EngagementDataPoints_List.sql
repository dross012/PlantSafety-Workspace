-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Engagement
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EngagementDataPoints_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Analytics].[EngagementDataPoints],
--                  backing IEngagementRepository.GetDataPointsAsync. Only the
--                  columns present on EngagementDataPointDto (Label, Value, Trend)
--                  are projected; TenantId/SiteId/RecordedAtUtc are filter-only.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_EngagementDataPoints_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [Label],
        [Value],
        [Trend]
FROM [Analytics].[EngagementDataPoints]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [RecordedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [RecordedAtUtc] DESC;
GO
