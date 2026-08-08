-- =============================================
-- Shared.SafetyData -- Phase2-SafetyTaxonomy
-- Script:        22-usp-get-requirements-by-regulator.sql
-- Purpose:       Creates SafetyTaxonomy.usp_GetRequirementsByRegulator
--                against [PlantSafety.CoreDB] -- mirrors
--                RegulatorRequirementCatalog.ForRegulator(regulator).
--
-- PARAMETER: @RegulatorCode is the exact, verbatim, hyphenated Code (e.g.
-- N'EPA-RCRA', N'NFPA-Dust', N'DHS-CFATS') recovered by
-- RegulatorExtensions.Code() in Regulator.cs -- the natural external key
-- for this lookup, since it is what a human-facing/API caller would supply.
-- To look up by the raw C# enum member name instead (e.g. N'EPA_RCRA'),
-- join on [SafetyTaxonomy].[Regulator].[EnumName] instead of [Code].
--
-- RESULT SET SHAPE: single result set, in RequirementId (catalog) order.
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

CREATE OR ALTER PROCEDURE [SafetyTaxonomy].[usp_GetRequirementsByRegulator]
    @RegulatorCode nvarchar(20)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @RegulatorCode IS NULL OR LTRIM(RTRIM(@RegulatorCode)) = N''
            THROW 50001, N'RegulatorCode is required.', 1;

        SELECT
            rr.[RequirementId],
            rr.[Name],
            rr.[Description],
            r.[RegulatorId],
            r.[EnumName],
            r.[Code],
            rc.[RegulatorCategoryId],
            rc.[CategoryName]
        FROM [SafetyTaxonomy].[RegulatoryRequirement] AS rr
        INNER JOIN [SafetyTaxonomy].[Regulator] AS r ON r.[RegulatorId] = rr.[RegulatorId]
        INNER JOIN [SafetyTaxonomy].[RegulatorCategory] AS rc ON rc.[RegulatorCategoryId] = rr.[RegulatorCategoryId]
        WHERE r.[Code] = @RegulatorCode
        ORDER BY rr.[RequirementId];
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext22 nvarchar(200) = CONCAT(N'Params=RegulatorCode=', @RegulatorCode, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails22 nvarchar(max) = CONCAT(
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
                SYSUTCDATETIME(), @ErrorContext22, @ErrorDetails22, DB_NAME(), OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO
