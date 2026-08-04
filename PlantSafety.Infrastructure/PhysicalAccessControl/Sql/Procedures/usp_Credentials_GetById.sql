-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.Credentials
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Credentials_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single Credential row by identity for aggregate rehydration.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_Credentials_GetById]
    @CredentialId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [CredentialId], [TenantId], [SiteId], [Status], [BadgeNumber], [HolderUserId],
        [HolderType], [AccessLevel], [IssuedAtUtc], [ExpiresAtUtc], [RevocationReason],
        [CreatedAtUtc], [CreatedBy]
    FROM [PhysicalAccessControl].[Credentials]
    WHERE [CredentialId] = @CredentialId
      AND ISNULL([IsDeleted], 0) = 0;
END
GO
