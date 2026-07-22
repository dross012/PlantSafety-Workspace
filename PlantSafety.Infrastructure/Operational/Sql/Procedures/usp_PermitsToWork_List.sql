-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.PermitsToWork
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PermitsToWork_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[PermitsToWorkReadModel],
--                  backing IPermitsToWorkRepository.GetPermitsToWorkAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_PermitsToWork_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PermitId],
        [TenantId],
        [SiteId],
        [PermitType],
        [WorkDescription],
        [Status],
        [IssuedAtUtc],
        [IssuedBy]
FROM [Operational].[PermitsToWorkReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [IssuedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [IssuedAtUtc] DESC;
GO
