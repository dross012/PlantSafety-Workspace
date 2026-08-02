-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PermitToWork
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PermitScheduling_GetRange.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IPermitSchedulingRepository.GetPermitSchedulingAsync
--                  (Application-layer read model). Additive-only evolution.
--  UPDATED:        2026-07-29 -- Projects RowVersion as a string ETag (Gap 2 remediation) so
--                  PermitSchedulingItemDto.ETag can round-trip back into a subsequent mutating
--                  command's IConcurrencyAwareCommand.ETag for read-before-write clients.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PermitToWork].[usp_PermitScheduling_GetRange]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT PermitSchedulingId, TenantId, SiteId, Status, PermitNumber, ScheduledStartUtc, ScheduledEndUtc, ConflictDetected, CreatedAtUtc,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [PermitToWork].[PermitScheduling]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND CreatedAtUtc >= @FromUtc AND CreatedAtUtc <= @ToUtc
      AND IsDeleted = 0
    ORDER BY CreatedAtUtc DESC;
END
GO
