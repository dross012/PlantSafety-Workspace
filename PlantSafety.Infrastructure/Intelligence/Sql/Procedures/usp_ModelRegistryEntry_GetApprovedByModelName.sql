-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.ModelRegistry
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ModelRegistryEntry_GetApprovedByModelName.sql
--  Created:        2026-08-20
--  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
--  SchemaVersion:  2026.1
--  Description:    Retrieves all Approved ModelRegistryEntry rows for a given (TenantId,
--                  ModelName), ordered by CreatedAtUtc descending. Backs
--                  IModelRegistryEntryRepository.GetApprovedByModelNameAsync (used by
--                  ModelRegistryDomainService.SupersedesCurrentDeployment callers to find the
--                  current production deployment).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Intelligence].[usp_ModelRegistryEntry_GetApprovedByModelName]
    @TenantId UNIQUEIDENTIFIER,
    @ModelName NVARCHAR(200)
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
        WHERE TenantId = @TenantId AND ModelName = @ModelName AND Status = N'Approved' AND IsDeleted = 0
        ORDER BY CreatedAtUtc DESC;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ModelName=', @ModelName, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
