-- =============================================
-- Shared.SafetyData -- Phase2-SafetyTaxonomy
-- Script:        23-usp-get-requirements-by-industry-name.sql
-- Purpose:       Creates SafetyTaxonomy.usp_GetRequirementsByIndustryName
--                against [PlantSafety.CoreDB] -- mirrors
--                RegulatorRequirementMapping.ByIndustryName(name): the union
--                of RegulatorRequirementCatalog.ForRegulator(r) for every
--                regulator r in the named industry's Regulators list, in
--                that list's source order.
--
-- RESULT SET SHAPE: single result set, ordered by the industry's
-- IndustryRegulator.OrdinalPosition (which regulator the requirement came
-- from, in the industry's own source-array order) then RequirementId
-- (each regulator's own requirements, in catalog order) -- mirroring
-- RegulatorRequirementMapping.For's SelectMany(industry.Regulators,
-- RegulatorRequirementCatalog.ForRegulator) iteration order exactly.
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

CREATE OR ALTER PROCEDURE [SafetyTaxonomy].[usp_GetRequirementsByIndustryName]
    @IndustryName nvarchar(200)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @IndustryName IS NULL OR LTRIM(RTRIM(@IndustryName)) = N''
            THROW 50001, N'IndustryName is required.', 1;

        SELECT
            rr.[RequirementId],
            rr.[Name],
            rr.[Description],
            r.[RegulatorId],
            r.[EnumName],
            r.[Code],
            rc.[RegulatorCategoryId],
            rc.[CategoryName],
            ir.[OrdinalPosition] AS [IndustryRegulatorOrdinalPosition]
        FROM [SafetyTaxonomy].[Industry] AS i
        INNER JOIN [SafetyTaxonomy].[IndustryRegulator] AS ir ON ir.[IndustryId] = i.[IndustryId]
        INNER JOIN [SafetyTaxonomy].[Regulator] AS r ON r.[RegulatorId] = ir.[RegulatorId]
        INNER JOIN [SafetyTaxonomy].[RegulatoryRequirement] AS rr ON rr.[RegulatorId] = r.[RegulatorId]
        INNER JOIN [SafetyTaxonomy].[RegulatorCategory] AS rc ON rc.[RegulatorCategoryId] = rr.[RegulatorCategoryId]
        WHERE i.[Name] = @IndustryName
        ORDER BY ir.[OrdinalPosition], rr.[RequirementId];
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext23 nvarchar(200) = CONCAT(N'Params=IndustryName=', @IndustryName, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails23 nvarchar(max) = CONCAT(
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
                SYSUTCDATETIME(), @ErrorContext23, @ErrorDetails23, DB_NAME(), OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO
