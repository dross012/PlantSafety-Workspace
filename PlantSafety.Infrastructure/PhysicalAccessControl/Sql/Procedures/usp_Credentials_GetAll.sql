-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.Credentials
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Credentials_GetAll.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves all non-deleted Credential rows for in-memory specification
--                  filtering by ListAsync (e.g. periodic credential-expiration sweep).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_Credentials_GetAll]
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [CredentialId], [TenantId], [SiteId], [Status], [BadgeNumber], [HolderUserId],
        [HolderType], [AccessLevel], [IssuedAtUtc], [ExpiresAtUtc], [RevocationReason],
        [CreatedAtUtc], [CreatedBy]
    FROM [PhysicalAccessControl].[Credentials]
    WHERE ISNULL([IsDeleted], 0) = 0;
END
GO
