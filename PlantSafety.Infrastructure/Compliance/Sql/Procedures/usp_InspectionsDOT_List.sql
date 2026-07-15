-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.InspectionsDOT
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_InspectionsDOT_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Compliance].[InspectionsDOT],
--                  backing IDOTRepository.GetInspectionsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_InspectionsDOT_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [InspectionId],
        [TenantId],
        [SiteId],
        [InspectionScope],
        [ScheduledAtUtc],
        [Status],
        [Inspector]
FROM [Compliance].[InspectionsDOT]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScheduledAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScheduledAtUtc] DESC;
GO