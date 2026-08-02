-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.DataLake
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_DataLakeActivity_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new Intelligence.DataLake DataLakeActivity aggregate root row. Backs
--                  IDataLakeActivityRepository.AddAsync (Domain write-side repository). TRY/CATCH
--                  with AuditDB/ErrorDB logging per the canonical write-procedure template (mirrors
--                  usp_RCA_Insert.sql).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Intelligence].[usp_DataLakeActivity_Insert]
    @DataLakeActivityId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @DatasetName NVARCHAR(200),
    @Zone NVARCHAR(50),
    @RecordCount BIGINT = NULL,
    @Status NVARCHAR(50),
    @ProcessedAtUtc DATETIME2(3),
    @ProcessedBy UNIQUEIDENTIFIER,
    @ProcessingStartedBy UNIQUEIDENTIFIER = NULL,
    @ProcessingStartedAtUtc DATETIME2(3) = NULL,
    @CuratedAtUtc DATETIME2(3) = NULL,
    @FailureReason NVARCHAR(MAX) = NULL,
    @FailedAtUtc DATETIME2(3) = NULL,
    @ArchivedAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Intelligence].[DataLakeActivity]
        (
            DataLakeActivityId, TenantId, SiteId, DatasetName, Zone, RecordCount, Status, ProcessedAtUtc, ProcessedBy,
            ProcessingStartedBy, ProcessingStartedAtUtc, CuratedAtUtc, FailureReason, FailedAtUtc, ArchivedAtUtc,
            CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @DataLakeActivityId, @TenantId, @SiteId, @DatasetName, @Zone, @RecordCount, @Status, @ProcessedAtUtc, @ProcessedBy,
            @ProcessingStartedBy, @ProcessingStartedAtUtc, @CuratedAtUtc, @FailureReason, @FailedAtUtc, @ArchivedAtUtc,
            @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'DataLakeActivity', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'DataLakeActivityId=', @DataLakeActivityId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'DataLakeActivityId=', @DataLakeActivityId, N'; User=', SYSTEM_USER);
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
