-- =============================================
-- Shared.SafetyData -- Phase2-SafetyTaxonomy
-- Script:        24-usp-get-programs-by-industry-name.sql
-- Purpose:       Creates SafetyTaxonomy.usp_GetProgramsByIndustryName
--                against [PlantSafety.CoreDB] -- mirrors
--                ProgramRequirementMapping.ByIndustryName(name), via
--                [SafetyTaxonomy].[fn_SafetyProgram_ApplicableToIndustry]
--                (19-fn-safetyprogram-applicabletoindustry.sql -- see that
--                file's header for the full applicability-rule derivation).
--
-- RESULT SET SHAPE: single result set, one row per applicable Safety
-- Program, ordered by ProgramId (== ProgramCatalog.All order) for
-- deterministic output.
--
-- SAFETY: SELECT only. Must never gain an INSERT/UPDATE/DELETE statement.
-- GOVERNANCE NOTE: no viewer-access AuditLog INSERT -- see
-- 20-usp-get-industries.sql's header for rationale (applies identically
-- here).
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [SafetyTaxonomy].[usp_GetProgramsByIndustryName]
    @IndustryName nvarchar(200)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @IndustryName IS NULL OR LTRIM(RTRIM(@IndustryName)) = N''
            THROW 50001, N'IndustryName is required.', 1;

        SELECT
            p.[ProgramId],
            p.[Code],
            p.[Name],
            p.[Description],
            p.[PrimaryRegulatorId],
            r.[EnumName]      AS [PrimaryRegulatorEnumName],
            r.[Code]           AS [PrimaryRegulatorCode],
            p.[RegulatorCategoryId],
            rc.[CategoryName],
            p.[PrimaryFlagId],
            f.[FlagName]       AS [PrimaryFlagName]
        FROM [SafetyTaxonomy].[fn_SafetyProgram_ApplicableToIndustry](@IndustryName) AS p
        INNER JOIN [SafetyTaxonomy].[Regulator] AS r ON r.[RegulatorId] = p.[PrimaryRegulatorId]
        INNER JOIN [SafetyTaxonomy].[RegulatorCategory] AS rc ON rc.[RegulatorCategoryId] = p.[RegulatorCategoryId]
        LEFT JOIN [SafetyTaxonomy].[SafetyFlag] AS f ON f.[SafetyFlagId] = p.[PrimaryFlagId]
        ORDER BY p.[ProgramId];
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext24 nvarchar(200) = CONCAT(N'Params=IndustryName=', @IndustryName, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails24 nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (
                ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                SYSUTCDATETIME(), @ErrorContext24, @ErrorDetails24, DB_NAME(), OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO
