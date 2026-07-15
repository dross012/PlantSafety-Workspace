-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Tracking
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Tracking_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Compliance].[Tracking],
--                  backing ITrackingRepository.GetTrackingAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_Tracking_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [TrackingItemId],
        [TenantId],
        [SiteId],
        [ItemDescription],
        [Category],
        [Status],
        [LastUpdatedAtUtc],
        [OwnerId]
FROM [Compliance].[Tracking]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [LastUpdatedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [LastUpdatedAtUtc] DESC;
GO