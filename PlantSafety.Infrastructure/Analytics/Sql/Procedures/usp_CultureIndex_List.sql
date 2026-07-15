-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Culture.CultureIndex
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_CultureIndex_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Analytics].[CultureIndex],
--                  backing ICultureIndexRepository.GetCultureIndexSnapshotsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_CultureIndex_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [IndexSnapshotId],
        [TenantId],
        [SiteId],
        [CultureDimension],
        [IndexScore],
        [TrendDirection],
        [Status],
        [CalculatedAtUtc]
FROM [Analytics].[CultureIndex]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [CalculatedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [CalculatedAtUtc] DESC;
GO
