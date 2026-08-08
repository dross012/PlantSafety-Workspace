-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.EPA
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Epa_GetThresholdRules.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Retrieves Compliance.EpaThresholdRule rows, optionally filtered by
--                  ProgramCode and/or ParameterName (NULL = no filter), ordered by
--                  ProgramCode/ParameterName. Global regulatory reference data.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_Epa_GetThresholdRules]
    @ProgramCode   NVARCHAR(20)  = NULL,
    @ParameterName NVARCHAR(200) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT ThresholdRuleId, ProgramCode, RequirementId, ParameterName, LimitValue, Unit,
               ConditionExpression, CreatedOn, RowVersion
        FROM [Compliance].[EpaThresholdRule]
        WHERE (@ProgramCode IS NULL OR ProgramCode = @ProgramCode)
          AND (@ParameterName IS NULL OR ParameterName = @ParameterName)
        ORDER BY ProgramCode, ParameterName;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ProgramCode=', @ProgramCode, N'; ParameterName=', @ParameterName, N'; User=', SYSTEM_USER);
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
