-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.LockoutTagout
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_LockoutTagout_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[LockoutTagoutReadModel],
--                  backing ILockoutTagoutRepository.GetLockoutTagoutAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_LockoutTagout_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [LockoutTagoutId],
        [TenantId],
        [SiteId],
        [EquipmentId],
        [LockType],
        [Status],
        [LockedAtUtc],
        [LockedBy]
FROM [Operational].[LockoutTagoutReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [LockedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [LockedAtUtc] DESC;
GO
