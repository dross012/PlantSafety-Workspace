-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Permissions
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Permissions_GetPermissions.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [CrossDomain].[Permissions],
--                  backing IPermissionsRepository.GetPermissionsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CrossDomain].[usp_Permissions_GetPermissions]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PermissionId],
        [TenantId],
        [SiteId],
        [PermissionName],
        [Scope],
        [GrantedToRoleId],
        [Status],
        [GrantedAtUtc],
        [GrantedBy]
FROM [CrossDomain].[Permissions]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [GrantedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [GrantedAtUtc] DESC;
GO
