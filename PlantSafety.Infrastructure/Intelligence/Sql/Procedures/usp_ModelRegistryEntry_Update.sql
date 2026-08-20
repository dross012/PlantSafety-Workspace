-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.ModelRegistry
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ModelRegistryEntry_Update.sql
--  Created:        2026-08-20
--  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a ModelRegistryEntry aggregate root row.
--                  Backs IModelRegistryEntryRepository.UpdateAsync (Domain write-side repository).
--                  @TenantId is part of the WHERE clause (tenant-scoped write) and @ETag enforces
--                  optimistic concurrency against the native RowVersion column, mirroring
--                  usp_DataLakeActivity_Update.sql's sibling proc. @RowsAffected is captured via
--                  @@ROWCOUNT immediately after the UPDATE (before the audit INSERT, which would
--                  otherwise reset it) and returned via SELECT -- callers MUST invoke this proc via
--                  ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync), since SET NOCOUNT ON
--                  makes ExecuteNonQuery always return -1, which would silently disable the @ETag
--                  concurrency check.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Intelligence].[usp_ModelRegistryEntry_Update]
    @ModelRegistryEntryId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @ApprovedBy UNIQUEIDENTIFIER = NULL,
    @ApprovedAtUtc DATETIME2(3) = NULL,
    @DeprecationReason NVARCHAR(1000) = NULL,
    @DeprecatedAtUtc DATETIME2(3) = NULL,
    @RetiredAtUtc DATETIME2(3) = NULL,
    @ModifiedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Intelligence].[ModelRegistryEntry] WHERE ModelRegistryEntryId = @ModelRegistryEntryId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'ModelRegistryEntry not found or has been deleted.', 1;

        UPDATE [Intelligence].[ModelRegistryEntry]
        SET
            Status = @Status,
            ApprovedBy = @ApprovedBy,
            ApprovedAtUtc = @ApprovedAtUtc,
            DeprecationReason = @DeprecationReason,
            DeprecatedAtUtc = @DeprecatedAtUtc,
            RetiredAtUtc = @RetiredAtUtc,
            ModifiedBy = @ModifiedBy,
            ModifiedAtUtc = SYSUTCDATETIME()
        WHERE ModelRegistryEntryId = @ModelRegistryEntryId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ModelRegistryEntry', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ModelRegistryEntryId=', @ModelRegistryEntryId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ModelRegistryEntryId=', @ModelRegistryEntryId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
