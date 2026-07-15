-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Users
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Users_GetUsers.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [CrossDomain].[Users],
--                  backing IUsersRepository.GetUsersAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CrossDomain].[usp_Users_GetUsers]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [UserId],
        [TenantId],
        [SiteId],
        [DisplayName],
        [Email],
        [RoleId],
        [Status],
        [LastLoginAtUtc],
        [CreatedBy]
FROM [CrossDomain].[Users]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [LastLoginAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [LastLoginAtUtc] DESC;
GO
