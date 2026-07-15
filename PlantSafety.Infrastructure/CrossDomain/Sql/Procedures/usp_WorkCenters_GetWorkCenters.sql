-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.WorkCenters
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_WorkCenters_GetWorkCenters.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [CrossDomain].[WorkCenters],
--                  backing IWorkCentersRepository.GetWorkCentersAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CrossDomain].[usp_WorkCenters_GetWorkCenters]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [WorkCenterId],
        [TenantId],
        [SiteId],
        [WorkCenterName],
        [WorkCenterCode],
        [Capacity],
        [Status],
        [LastUpdatedAtUtc],
        [LastUpdatedBy]
FROM [CrossDomain].[WorkCenters]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [LastUpdatedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [LastUpdatedAtUtc] DESC;
GO
