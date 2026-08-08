-- =============================================
-- Shared.SafetyData -- Phase2-SafetyTaxonomy
-- Script:        20-usp-get-industries.sql
-- Purpose:       Creates SafetyTaxonomy.usp_GetIndustries against
--                [PlantSafety.CoreDB], the read-only query path over
--                [SafetyTaxonomy].[Industry] (denormalized with its
--                SubIndustries/Regulators/Flags children) -- the SQL mirror
--                of IndustryRegistry.All.
--
-- RESULT SET SHAPE: single, flat result set, one row per industry, with
-- SubIndustries/Regulators/Flags collapsed to comma-separated
-- STRING_AGG(...) columns rather than separate child result sets --
-- reference/lookup callers (e.g. populating a dropdown of all 40
-- industries) typically want the whole taxonomy in one round trip, unlike
-- the paginated business-aggregate procs in Phase1-StoredProcedures. Each
-- STRING_AGG carries an explicit WITHIN GROUP (ORDER BY ...) so the
-- aggregation itself is deterministic (validate-determinism), matching each
-- child table's own source-array order (OrdinalPosition for
-- SubIndustries/Regulators; SafetyFlagId for Flags, since Flags has no
-- ordered-array analog in the C# source -- see 16-seed-industryflag.sql's
-- header).
--
-- GOVERNANCE NOTE: no viewer-access AuditLog INSERT here (unlike the
-- canonical stored-procedure template) -- this procedure reads static,
-- non-tenant, non-sensitive reference/master data (the 40-industry
-- taxonomy), not tenant-owned business data, so per-call audit logging of
-- "who viewed the industry list" would be governance noise, not a
-- meaningful control. Error logging is still retained (CATCH block below)
-- so failures remain diagnosable.
--
-- SAFETY: SELECT only. This procedure must never gain an INSERT/UPDATE/
-- DELETE statement -- SafetyTaxonomy.Industry is owned exclusively by
-- Shared.SafetyData.Abstractions.Models.Industries.IndustryRegistry (C#)
-- and its mirror is populated exclusively by 13-seed-industry.sql et al.
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [SafetyTaxonomy].[usp_GetIndustries]
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT
            i.[IndustryId],
            i.[Name],
            i.[TopLevelCategory],
            (
                SELECT STRING_AGG(si.[SubIndustryName], N', ') WITHIN GROUP (ORDER BY si.[OrdinalPosition])
                FROM [SafetyTaxonomy].[IndustrySubIndustry] AS si
                WHERE si.[IndustryId] = i.[IndustryId]
            ) AS [SubIndustries],
            (
                SELECT STRING_AGG(r.[Code], N', ') WITHIN GROUP (ORDER BY ir.[OrdinalPosition])
                FROM [SafetyTaxonomy].[IndustryRegulator] AS ir
                INNER JOIN [SafetyTaxonomy].[Regulator] AS r ON r.[RegulatorId] = ir.[RegulatorId]
                WHERE ir.[IndustryId] = i.[IndustryId]
            ) AS [Regulators],
            (
                SELECT STRING_AGG(f.[FlagName], N', ') WITHIN GROUP (ORDER BY f.[SafetyFlagId])
                FROM [SafetyTaxonomy].[IndustryFlag] AS ifl
                INNER JOIN [SafetyTaxonomy].[SafetyFlag] AS f ON f.[SafetyFlagId] = ifl.[SafetyFlagId]
                WHERE ifl.[IndustryId] = i.[IndustryId]
            ) AS [Flags]
        FROM [SafetyTaxonomy].[Industry] AS i
        ORDER BY i.[IndustryId];
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext20 nvarchar(200) = CONCAT(N'Procedure=SafetyTaxonomy.usp_GetIndustries; User=', SYSTEM_USER);
        DECLARE @ErrorDetails20 nvarchar(max) = CONCAT(
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
                SYSUTCDATETIME(), @ErrorContext20, @ErrorDetails20, DB_NAME(), OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO
