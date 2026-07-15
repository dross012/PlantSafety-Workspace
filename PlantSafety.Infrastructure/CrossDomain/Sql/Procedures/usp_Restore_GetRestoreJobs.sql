-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Restore
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Restore_GetRestoreJobs.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [CrossDomain].[Restore],
--                  backing IRestoreRepository.GetRestoreJobsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CrossDomain].[usp_Restore_GetRestoreJobs]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [RestoreJobId],
        [TenantId],
        [SiteId],
        [SourceBackupId],
        [RestoreType],
        [TargetLocation],
        [Status],
        [CompletedAtUtc],
        [InitiatedBy]
FROM [CrossDomain].[Restore]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [CompletedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [CompletedAtUtc] DESC;
GO
