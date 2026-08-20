-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.FeatureEngineering
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_FeatureSet_Insert.sql
--  Created:        2026-08-20
--  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new Intelligence.FeatureEngineering FeatureSet entity row. Backs
--                  IFeatureSetRepository.AddAsync (Domain write-side repository). TRY/CATCH with
--                  AuditDB/ErrorDB logging per the canonical write-procedure template (mirrors
--                  usp_DataLakeActivity_Insert.sql).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Intelligence].[usp_FeatureSet_Insert]
    @FeatureSetId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Name NVARCHAR(200),
    @Version NVARCHAR(16),
    @DataSourcePath NVARCHAR(500),
    @IsPublished BIT,
    @FeatureNamesCsv NVARCHAR(MAX),
    @ComputedAtUtc DATETIME2(3),
    @PublishedAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Intelligence].[FeatureSet]
        (
            FeatureSetId, TenantId, SiteId, Name, Version, DataSourcePath, IsPublished, FeatureNamesCsv,
            ComputedAtUtc, PublishedAtUtc, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @FeatureSetId, @TenantId, @SiteId, @Name, @Version, @DataSourcePath, @IsPublished, @FeatureNamesCsv,
            @ComputedAtUtc, @PublishedAtUtc, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'FeatureSet', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'FeatureSetId=', @FeatureSetId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
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
