-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.FeatureEngineering
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_FeatureSet_GetLatestPublishedByName.sql
--  Created:        2026-08-20
--  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
--  SchemaVersion:  2026.1
--  Description:    Retrieves the most recently published FeatureSet row for a given (TenantId,
--                  Name), ordered by PublishedAtUtc descending, TOP 1. Backs
--                  IFeatureSetRepository.GetLatestPublishedAsync (used by
--                  FeatureEngineeringDomainService.Supersedes callers to determine whether a new
--                  version supersedes the current published one).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Intelligence].[usp_FeatureSet_GetLatestPublishedByName]
    @TenantId UNIQUEIDENTIFIER,
    @Name NVARCHAR(200)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT TOP (1)
            FeatureSetId, TenantId, SiteId, Name, Version, DataSourcePath, IsPublished, FeatureNamesCsv,
            ComputedAtUtc, PublishedAtUtc, CreatedAtUtc, CreatedBy, ModifiedAtUtc, ModifiedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Intelligence].[FeatureSet]
        WHERE TenantId = @TenantId AND Name = @Name AND IsPublished = 1 AND IsDeleted = 0
        ORDER BY PublishedAtUtc DESC;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'Name=', @Name, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
