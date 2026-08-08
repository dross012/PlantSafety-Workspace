-- =============================================
-- Shared.SafetyData -- Phase2-SafetyTaxonomy
-- Script:        25-usp-get-programs-by-regulator.sql
-- Purpose:       Creates SafetyTaxonomy.usp_GetProgramsByRegulator against
--                [PlantSafety.CoreDB] -- mirrors ProgramCatalog.ForRegulator
--                (Shared.SafetyData.Abstractions\Models\Industries\
--                ProgramCatalog.cs).
--
-- PARAMETER: @RegulatorCode is the exact, verbatim, hyphenated Code (see
-- 22-usp-get-requirements-by-regulator.sql's header for the same note).
--
-- RESULT SET SHAPE: single result set, in ProgramId (== ProgramCatalog.All
-- order) order.
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

CREATE OR ALTER PROCEDURE [SafetyTaxonomy].[usp_GetProgramsByRegulator]
    @RegulatorCode nvarchar(20)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @RegulatorCode IS NULL OR LTRIM(RTRIM(@RegulatorCode)) = N''
            THROW 50001, N'RegulatorCode is required.', 1;

        SELECT
            p.[ProgramId],
            p.[Code],
            p.[Name],
            p.[Description],
            r.[RegulatorId],
            r.[EnumName],
            r.[Code]           AS [RegulatorCode],
            rc.[RegulatorCategoryId],
            rc.[CategoryName],
            p.[PrimaryFlagId],
            f.[FlagName]       AS [PrimaryFlagName]
        FROM [SafetyTaxonomy].[SafetyProgram] AS p
        INNER JOIN [SafetyTaxonomy].[Regulator] AS r ON r.[RegulatorId] = p.[PrimaryRegulatorId]
        INNER JOIN [SafetyTaxonomy].[RegulatorCategory] AS rc ON rc.[RegulatorCategoryId] = p.[RegulatorCategoryId]
        LEFT JOIN [SafetyTaxonomy].[SafetyFlag] AS f ON f.[SafetyFlagId] = p.[PrimaryFlagId]
        WHERE r.[Code] = @RegulatorCode
        ORDER BY p.[ProgramId];
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext25 nvarchar(200) = CONCAT(N'Params=RegulatorCode=', @RegulatorCode, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails25 nvarchar(max) = CONCAT(
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
                SYSUTCDATETIME(), @ErrorContext25, @ErrorDetails25, DB_NAME(), OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO
