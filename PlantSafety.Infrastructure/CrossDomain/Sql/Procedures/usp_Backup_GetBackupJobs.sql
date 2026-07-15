-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Backup
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Backup_GetBackupJobs.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [CrossDomain].[Backup],
--                  backing IBackupRepository.GetBackupJobsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CrossDomain].[usp_Backup_GetBackupJobs]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [BackupJobId],
        [TenantId],
        [SiteId],
        [BackupType],
        [TargetLocation],
        [SizeBytes],
        [Status],
        [CompletedAtUtc],
        [InitiatedBy]
FROM [CrossDomain].[Backup]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [CompletedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [CompletedAtUtc] DESC;
GO
