-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Roles
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Roles_GetRoles.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [CrossDomain].[Roles],
--                  backing IRolesRepository.GetRolesAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CrossDomain].[usp_Roles_GetRoles]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [RoleId],
        [TenantId],
        [SiteId],
        [RoleName],
        [Description],
        [PermissionCount],
        [Status],
        [CreatedAtUtc],
        [CreatedBy]
FROM [CrossDomain].[Roles]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [CreatedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [CreatedAtUtc] DESC;
GO
