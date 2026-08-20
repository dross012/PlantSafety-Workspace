-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.ModelRegistry
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ModelRegistryEntry_Insert.sql
--  Created:        2026-08-20
--  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new Intelligence.ModelRegistry ModelRegistryEntry aggregate root row.
--                  Backs IModelRegistryEntryRepository.AddAsync (Domain write-side repository).
--                  TRY/CATCH with AuditDB/ErrorDB logging per the canonical write-procedure
--                  template (mirrors usp_DataLakeActivity_Insert.sql).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Intelligence].[usp_ModelRegistryEntry_Insert]
    @ModelRegistryEntryId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @ModelName NVARCHAR(200),
    @ModelVersion NVARCHAR(32),
    @Status NVARCHAR(50),
    @DatasetHash NVARCHAR(64),
    @TrainingAccuracyScore FLOAT,
    @RegisteredBy UNIQUEIDENTIFIER,
    @ApprovedBy UNIQUEIDENTIFIER = NULL,
    @ApprovedAtUtc DATETIME2(3) = NULL,
    @DeprecationReason NVARCHAR(1000) = NULL,
    @DeprecatedAtUtc DATETIME2(3) = NULL,
    @RetiredAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Intelligence].[ModelRegistryEntry]
        (
            ModelRegistryEntryId, TenantId, SiteId, ModelName, ModelVersion, Status, DatasetHash, TrainingAccuracyScore,
            RegisteredBy, ApprovedBy, ApprovedAtUtc, DeprecationReason, DeprecatedAtUtc, RetiredAtUtc,
            CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @ModelRegistryEntryId, @TenantId, @SiteId, @ModelName, @ModelVersion, @Status, @DatasetHash, @TrainingAccuracyScore,
            @RegisteredBy, @ApprovedBy, @ApprovedAtUtc, @DeprecationReason, @DeprecatedAtUtc, @RetiredAtUtc,
            @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ModelRegistryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ModelRegistryEntryId=', @ModelRegistryEntryId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
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
