-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Updates
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Updates_GetUpdates.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [CrossDomain].[Updates],
--                  backing IUpdatesRepository.GetUpdatesAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CrossDomain].[usp_Updates_GetUpdates]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [UpdateId],
        [TenantId],
        [SiteId],
        [ComponentName],
        [VersionFrom],
        [VersionTo],
        [Status],
        [AppliedAtUtc],
        [AppliedBy]
FROM [CrossDomain].[Updates]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [AppliedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [AppliedAtUtc] DESC;
GO
