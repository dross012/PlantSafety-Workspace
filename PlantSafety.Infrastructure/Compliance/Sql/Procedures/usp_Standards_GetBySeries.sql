-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Standards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Standards_GetBySeries.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Retrieves paged Compliance.StandardsRequirement rows for a given
--                  (FamilyCode, SeriesCode), ordered by Title. Global regulatory-adjacent
--                  reference data. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_Standards_GetBySeries]
    @FamilyCode NVARCHAR(10),
    @SeriesCode NVARCHAR(30),
    @Skip       INT = 0,
    @Take       INT = 500
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT RequirementId, FamilyCode, SeriesCode, Title, Description, ApplicabilityConditions,
               ImpactedModules, Severity, SourceUrl, CreatedOn, RowVersion
        FROM [Compliance].[StandardsRequirement]
        WHERE FamilyCode = @FamilyCode AND SeriesCode = @SeriesCode
        ORDER BY Title
        OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'FamilyCode=', @FamilyCode, N'; SeriesCode=', @SeriesCode, N'; User=', SYSTEM_USER);
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
