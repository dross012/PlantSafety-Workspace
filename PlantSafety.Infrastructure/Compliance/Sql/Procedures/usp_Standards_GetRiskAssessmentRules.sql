-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Standards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Standards_GetRiskAssessmentRules.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Retrieves Compliance.StandardsRiskAssessmentRule rows, optionally
--                  filtered by Methodology and/or RequirementId (both optional, combinable).
--                  Global regulatory-adjacent reference data. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_Standards_GetRiskAssessmentRules]
    @Methodology   NVARCHAR(100)    = NULL,
    @RequirementId UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT RiskAssessmentRuleId, RequirementId, Methodology, HazardIdentificationSteps,
               RiskReductionMeasures, Description, CreatedOn, RowVersion
        FROM [Compliance].[StandardsRiskAssessmentRule]
        WHERE (@Methodology IS NULL OR Methodology = @Methodology)
          AND (@RequirementId IS NULL OR RequirementId = @RequirementId)
        ORDER BY Methodology;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'Methodology=', @Methodology, N'; RequirementId=', @RequirementId, N'; User=', SYSTEM_USER);
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
