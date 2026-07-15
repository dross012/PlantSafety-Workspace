-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Cleanup
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Cleanup_GetCleanupTasks.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [CrossDomain].[Cleanup],
--                  backing ICleanupRepository.GetCleanupTasksAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CrossDomain].[usp_Cleanup_GetCleanupTasks]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [CleanupTaskId],
        [TenantId],
        [SiteId],
        [TargetArea],
        [ItemsRemoved],
        [ReclaimedBytes],
        [Status],
        [ExecutedAtUtc],
        [ExecutedBy]
FROM [CrossDomain].[Cleanup]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ExecutedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ExecutedAtUtc] DESC;
GO
