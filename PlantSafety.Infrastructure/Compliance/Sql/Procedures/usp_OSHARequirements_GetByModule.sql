-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.OSHAStandards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_OSHARequirements_GetByModule.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Retrieves every Compliance.OSHARequirement row whose AppliesToModule matches
--                  exactly. Returns zero rows until a separate mapping pass populates
--                  AppliesToModule -- tools/OshaStandardsImport always leaves it NULL. Backs
--                  IOSHAStandardsRepository.GetRequirementsByModuleAsync. Additive-only
--                  evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_OSHARequirements_GetByModule]
    @AppliesToModule NVARCHAR(100)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT OSHARequirementId, OSHAStandardId, RequirementType, AppliesToModule, Severity, Notes, CreatedAtUtc
        FROM [Compliance].[OSHARequirement]
        WHERE AppliesToModule = @AppliesToModule
        ORDER BY CreatedAtUtc;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AppliesToModule=', @AppliesToModule, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO
