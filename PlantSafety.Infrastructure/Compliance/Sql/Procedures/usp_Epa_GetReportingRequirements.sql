-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.EPA
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Epa_GetReportingRequirements.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Retrieves Compliance.EpaReportingRequirement rows, optionally filtered
--                  by ProgramCode (NULL = no filter), ordered by ProgramCode. Global
--                  regulatory reference data. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_Epa_GetReportingRequirements]
    @ProgramCode NVARCHAR(20) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT ReportingRequirementId, ProgramCode, RequirementId, TriggerDescription,
               DeadlineDescription, ReportingDestination, FormName, CreatedOn, RowVersion
        FROM [Compliance].[EpaReportingRequirement]
        WHERE (@ProgramCode IS NULL OR ProgramCode = @ProgramCode)
        ORDER BY ProgramCode;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ProgramCode=', @ProgramCode, N'; User=', SYSTEM_USER);
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
