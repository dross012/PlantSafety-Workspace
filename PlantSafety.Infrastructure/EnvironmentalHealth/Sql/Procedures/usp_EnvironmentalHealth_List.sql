-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EnvironmentalHealth_List.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Deterministic paginated list procedure for
--                  [EnvironmentalHealth].[EnvironmentalHealthReadings], backing
--                  IEnvironmentalHealthReadingRepository.ListAsync. Explicit ORDER BY +
--                  OFFSET/FETCH paging; windowed TotalCount column avoids a second round trip.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_EnvironmentalHealth_List]
    @TenantId       UNIQUEIDENTIFIER,
    @SiteId         UNIQUEIDENTIFIER = NULL,
    @MetricType     NVARCHAR(50)     = NULL,
    @Status         NVARCHAR(20)     = NULL,
    @FromUtc        DATETIME2(3)     = NULL,
    @ToUtc          DATETIME2(3)     = NULL,
    @OrderBy        NVARCHAR(100)    = 'RecordedAtUtc',
    @SortDirection  NVARCHAR(4)      = 'DESC',
    @PageNumber     INT              = 1,
    @PageSize       INT              = 25
AS
SET NOCOUNT ON;

SELECT
        [EnvironmentalHealthId],
        [TenantId],
        [SiteId],
        [MetricType],
        [MeasurementValue],
        [Unit],
        [Threshold],
        [Status],
        [RecordedAtUtc],
        COUNT(*) OVER() AS [TotalCount]
FROM [EnvironmentalHealth].[EnvironmentalHealthReadings]
WHERE [TenantId] = @TenantId
  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
  AND (@MetricType IS NULL OR [MetricType] = @MetricType)
  AND (@Status IS NULL OR [Status] = @Status)
  AND (@FromUtc IS NULL OR [RecordedAtUtc] >= @FromUtc)
  AND (@ToUtc IS NULL OR [RecordedAtUtc] <= @ToUtc)
ORDER BY
    CASE WHEN @OrderBy = 'RecordedAtUtc' AND @SortDirection = 'ASC'  THEN [RecordedAtUtc] END ASC,
    CASE WHEN @OrderBy = 'RecordedAtUtc' AND @SortDirection = 'DESC' THEN [RecordedAtUtc] END DESC,
    CASE WHEN @OrderBy = 'MetricType'    AND @SortDirection = 'ASC'  THEN [MetricType] END ASC,
    CASE WHEN @OrderBy = 'MetricType'    AND @SortDirection = 'DESC' THEN [MetricType] END DESC,
    CASE WHEN @OrderBy = 'Status'        AND @SortDirection = 'ASC'  THEN [Status] END ASC,
    CASE WHEN @OrderBy = 'Status'        AND @SortDirection = 'DESC' THEN [Status] END DESC,
    [RecordedAtUtc] DESC
OFFSET (@PageNumber - 1) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY;
GO
