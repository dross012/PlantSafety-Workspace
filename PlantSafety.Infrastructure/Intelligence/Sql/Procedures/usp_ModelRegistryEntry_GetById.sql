-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.ModelRegistry
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ModelRegistryEntry_GetById.sql
--  Created:        2026-08-20
--  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single ModelRegistryEntry aggregate root row by identity,
--                  tenant-scoped. Backs IModelRegistryEntryRepository.GetByIdAsync (Domain
--                  write-side aggregate hydration) and the Application-layer
--                  GetModelRegistryEntryByIdQuery read path. Canonical TRY/CATCH + ErrorDB
--                  logging (no AuditLog for reads). Projects RowVersion as a string ETag. Mirrors
--                  usp_DataLakeActivity_GetById.sql.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Intelligence].[usp_ModelRegistryEntry_GetById]
    @ModelRegistryEntryId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            ModelRegistryEntryId, TenantId, SiteId, ModelName, ModelVersion, Status, DatasetHash, TrainingAccuracyScore,
            RegisteredBy, ApprovedBy, ApprovedAtUtc, DeprecationReason, DeprecatedAtUtc, RetiredAtUtc,
            CreatedAtUtc, CreatedBy, ModifiedAtUtc, ModifiedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Intelligence].[ModelRegistryEntry]
        WHERE ModelRegistryEntryId = @ModelRegistryEntryId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ModelRegistryEntryId=', @ModelRegistryEntryId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
