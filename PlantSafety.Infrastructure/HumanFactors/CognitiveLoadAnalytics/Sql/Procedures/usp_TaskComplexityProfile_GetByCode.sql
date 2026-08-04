-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HumanFactors.CognitiveLoadAnalytics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_TaskComplexityProfile_GetByCode.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Retrieves the most recently defined TaskComplexityProfile for a task code
--                  within a tenant. Backs GetTaskComplexityProfileQuery.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_TaskComplexityProfile_GetByCode]
    @TenantId UNIQUEIDENTIFIER,
    @TaskCode NVARCHAR(50)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT TOP (1)
            TaskComplexityProfileId, CognitiveLoadLedgerId, TaskCode, TaskName, ComplexityScore, Description, DefinedAtUtc
        FROM [HumanFactorsCognitiveLoadAnalytics].[TaskComplexityProfile]
        WHERE TenantId = @TenantId AND TaskCode = @TaskCode
        ORDER BY DefinedAtUtc DESC;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
