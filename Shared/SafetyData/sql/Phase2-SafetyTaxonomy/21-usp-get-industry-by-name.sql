-- =============================================
-- Shared.SafetyData -- Phase2-SafetyTaxonomy
-- Script:        21-usp-get-industry-by-name.sql
-- Purpose:       Creates SafetyTaxonomy.usp_GetIndustryByName against
--                [PlantSafety.CoreDB] -- the single-industry detail read
--                path, mirroring IndustryRegistry.FindByName(name).
--
-- RESULT SET SHAPE: four result sets, mirroring
-- Phase1-StoredProcedures\01-usp-get-hazard-summary.sql's multi-result-set
-- precedent for a one-to-many parent+children shape:
--   (1) the single matching Industry row (zero rows if no such industry)
--   (2) its SubIndustries, in source order
--   (3) its Regulators (joined to Regulator for Code/Description), in
--       source order
--   (4) its Flags (joined to SafetyFlag for FlagName/BitValue), in
--       SafetyFlagId order
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

CREATE OR ALTER PROCEDURE [SafetyTaxonomy].[usp_GetIndustryByName]
    @IndustryName nvarchar(200)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @IndustryName IS NULL OR LTRIM(RTRIM(@IndustryName)) = N''
            THROW 50001, N'IndustryName is required.', 1;

        ------------------------------------------------------------
        -- Result set 1: the matching Industry row
        ------------------------------------------------------------
        SELECT i.[IndustryId], i.[Name], i.[TopLevelCategory]
        FROM [SafetyTaxonomy].[Industry] AS i
        WHERE i.[Name] = @IndustryName;

        ------------------------------------------------------------
        -- Result set 2: its SubIndustries, in source order
        ------------------------------------------------------------
        SELECT si.[SubIndustryName], si.[OrdinalPosition]
        FROM [SafetyTaxonomy].[IndustrySubIndustry] AS si
        INNER JOIN [SafetyTaxonomy].[Industry] AS i ON i.[IndustryId] = si.[IndustryId]
        WHERE i.[Name] = @IndustryName
        ORDER BY si.[OrdinalPosition];

        ------------------------------------------------------------
        -- Result set 3: its Regulators, in source order
        ------------------------------------------------------------
        SELECT r.[RegulatorId], r.[EnumName], r.[Code], r.[Description], ir.[OrdinalPosition]
        FROM [SafetyTaxonomy].[IndustryRegulator] AS ir
        INNER JOIN [SafetyTaxonomy].[Industry] AS i ON i.[IndustryId] = ir.[IndustryId]
        INNER JOIN [SafetyTaxonomy].[Regulator] AS r ON r.[RegulatorId] = ir.[RegulatorId]
        WHERE i.[Name] = @IndustryName
        ORDER BY ir.[OrdinalPosition];

        ------------------------------------------------------------
        -- Result set 4: its Flags, in SafetyFlagId order
        ------------------------------------------------------------
        SELECT f.[SafetyFlagId], f.[FlagName], f.[BitValue]
        FROM [SafetyTaxonomy].[IndustryFlag] AS ifl
        INNER JOIN [SafetyTaxonomy].[Industry] AS i ON i.[IndustryId] = ifl.[IndustryId]
        INNER JOIN [SafetyTaxonomy].[SafetyFlag] AS f ON f.[SafetyFlagId] = ifl.[SafetyFlagId]
        WHERE i.[Name] = @IndustryName
        ORDER BY f.[SafetyFlagId];
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext21 nvarchar(200) = CONCAT(N'Params=IndustryName=', @IndustryName, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails21 nvarchar(max) = CONCAT(
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
                SYSUTCDATETIME(), @ErrorContext21, @ErrorDetails21, DB_NAME(), OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO
