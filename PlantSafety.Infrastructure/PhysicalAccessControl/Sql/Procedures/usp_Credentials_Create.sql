-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.Credentials
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Credentials_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Inserts a new Credential aggregate row. Best-effort audit-log side effect
--                  (badge/credential PII access-restricted and audit-logged).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_Credentials_Create]
    @CredentialId  UNIQUEIDENTIFIER,
    @TenantId      UNIQUEIDENTIFIER,
    @SiteId        UNIQUEIDENTIFIER = NULL,
    @Status        NVARCHAR(100),
    @BadgeNumber   NVARCHAR(100),
    @HolderUserId  UNIQUEIDENTIFIER,
    @HolderType    NVARCHAR(100),
    @AccessLevel   NVARCHAR(100),
    @IssuedAtUtc   DATETIME2(3),
    @ExpiresAtUtc  DATETIME2(3) = NULL,
    @CreatedAtUtc  DATETIME2(3),
    @CreatedBy     UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [PhysicalAccessControl].[Credentials]
            ([CredentialId], [TenantId], [SiteId], [Status], [BadgeNumber], [HolderUserId],
             [HolderType], [AccessLevel], [IssuedAtUtc], [ExpiresAtUtc], [CreatedAtUtc], [CreatedBy])
        VALUES
            (@CredentialId, @TenantId, @SiteId, @Status, @BadgeNumber, @HolderUserId,
             @HolderType, @AccessLevel, @IssuedAtUtc, @ExpiresAtUtc, @CreatedAtUtc, @CreatedBy);

        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime)
        VALUES ('PhysicalAccessControl.Credentials', DB_NAME(), 'INSERT', @CreatedBy, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
