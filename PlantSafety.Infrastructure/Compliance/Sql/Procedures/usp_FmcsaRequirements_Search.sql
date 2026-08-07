-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.FMCSAStandards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_FmcsaRequirements_Search.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Free-text (LIKE) search over Compliance.FMCSARequirement.RequirementText,
--                  paged. Backs IFmcsaStandardsRepository.SearchRequirementsAsync. Mirrors
--                  usp_OSHARequirements_Search.sql. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_FmcsaRequirements_Search]
    @Keyword NVARCHAR(400),
    @Skip    INT,
    @Take    INT
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT RequirementId, StandardId, RequirementNumber, RequirementText, Severity, AppliesToModule, Rationale, RowVersion
        FROM [Compliance].[FMCSARequirement]
        WHERE RequirementText LIKE N'%' + @Keyword + N'%'
        ORDER BY RequirementNumber
        OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'Keyword=', @Keyword, N'; User=', SYSTEM_USER);
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
