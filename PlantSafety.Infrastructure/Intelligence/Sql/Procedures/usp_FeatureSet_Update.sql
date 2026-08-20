-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.FeatureEngineering
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_FeatureSet_Update.sql
--  Created:        2026-08-20
--  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a FeatureSet entity row (feature list,
--                  publish status). Backs IFeatureSetRepository.UpdateAsync (Domain write-side
--                  repository). @TenantId is part of the WHERE clause (tenant-scoped write) and
--                  @ETag enforces optimistic concurrency against the native RowVersion column,
--                  mirroring usp_DataLakeActivity_Update.sql's sibling proc. @RowsAffected is
--                  captured via @@ROWCOUNT immediately after the UPDATE (before the audit INSERT,
--                  which would otherwise reset it) and returned via SELECT -- callers MUST invoke
--                  this proc via ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync), since SET
--                  NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
--                  the @ETag concurrency check.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Intelligence].[usp_FeatureSet_Update]
    @FeatureSetId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @IsPublished BIT,
    @FeatureNamesCsv NVARCHAR(MAX),
    @PublishedAtUtc DATETIME2(3) = NULL,
    @ModifiedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Intelligence].[FeatureSet] WHERE FeatureSetId = @FeatureSetId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'FeatureSet not found or has been deleted.', 1;

        UPDATE [Intelligence].[FeatureSet]
        SET
            IsPublished = @IsPublished,
            FeatureNamesCsv = @FeatureNamesCsv,
            PublishedAtUtc = @PublishedAtUtc,
            ModifiedBy = @ModifiedBy,
            ModifiedAtUtc = SYSUTCDATETIME()
        WHERE FeatureSetId = @FeatureSetId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'FeatureSet', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'FeatureSetId=', @FeatureSetId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'FeatureSetId=', @FeatureSetId, N'; User=', SYSTEM_USER);
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
