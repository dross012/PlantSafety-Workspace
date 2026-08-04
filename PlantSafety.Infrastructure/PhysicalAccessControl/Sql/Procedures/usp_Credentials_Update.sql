-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.Credentials
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Credentials_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Updates an existing Credential aggregate row. Best-effort audit-log side
--                  effect.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_Credentials_Update]
    @NowUtc            DATETIME2(3),
    @CredentialId      UNIQUEIDENTIFIER,
    @TenantId          UNIQUEIDENTIFIER,
    @SiteId            UNIQUEIDENTIFIER = NULL,
    @Status            NVARCHAR(100),
    @BadgeNumber       NVARCHAR(100),
    @HolderUserId      UNIQUEIDENTIFIER,
    @HolderType        NVARCHAR(100),
    @AccessLevel       NVARCHAR(100),
    @ExpiresAtUtc      DATETIME2(3) = NULL,
    @RevocationReason  NVARCHAR(1000) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [PhysicalAccessControl].[Credentials]
        SET [Status]           = @Status,
            [BadgeNumber]      = @BadgeNumber,
            [HolderUserId]     = @HolderUserId,
            [HolderType]       = @HolderType,
            [AccessLevel]      = @AccessLevel,
            [ExpiresAtUtc]     = @ExpiresAtUtc,
            [RevocationReason] = @RevocationReason,
            [ModifiedAtUtc]    = @NowUtc
        WHERE [CredentialId] = @CredentialId
          AND [TenantId] = @TenantId;

        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime)
        VALUES ('PhysicalAccessControl.Credentials', DB_NAME(), 'UPDATE', NULL, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
