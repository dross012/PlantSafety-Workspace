-- =============================================
-- Shared.SafetyData -- Phase2-SafetyTaxonomy
-- Script:        19-fn-safetyprogram-applicabletoindustry.sql
-- Purpose:       Creates SafetyTaxonomy.fn_SafetyProgram_ApplicableToIndustry
--                against [PlantSafety.CoreDB] -- an inline table-valued
--                function (deterministic, single RETURN SELECT, no
--                nondeterministic functions) mirroring
--                ProgramRequirementMapping.For(industry) (Shared.SafetyData.
--                Abstractions\Models\Industries\ProgramRequirementMapping.cs).
--
-- DERIVATION (per explicit task instruction -- "a program applies if its
-- regulator is in the industry's regulator list OR its primary flag is in
-- the industry's flag list"): this is a faithful simplification of
-- ProgramRequirementMapping.AppliesTo's two real C# paths, not an
-- approximation --
--   - regulatorApplies (C#): RegulatorRequirementMapping.For(industry)
--     contains a requirement with the same Regulator+Name as the program's
--     source requirement. Since ProgramCatalog.All is built 1:1 from
--     RegulatorRequirementCatalog.All (every program has exactly one source
--     requirement, under exactly one regulator), and
--     RegulatorRequirementMapping.For(industry) is literally "every
--     requirement for every regulator the industry carries", this reduces
--     exactly to "industry.Regulators contains program.PrimaryRegulator" --
--     ProgramRequirementMapping.cs's own header comment confirms this ("...
--     equivalent to, but more traceable than, a raw
--     industry.Regulators.Contains check").
--   - flagApplies (C#): requires HazardMapping.For(industry) AND
--     RequiredSafetyModules.For(industry) both agree the flag applies; per
--     that same header comment, both are "independently derived from the
--     same SafetyFlagCatalog.Decompose(industry.Flags)", so the conjunction
--     reduces exactly to "program.PrimaryFlag is in
--     SafetyFlagCatalog.Decompose(industry.Flags)" -- i.e.
--     "industry.Flags carries program.PrimaryFlag" -- which is exactly what
--     [SafetyTaxonomy].[IndustryFlag] enumerates.
-- A program applies if either path holds (LEFT JOIN + OR below), matching
-- ProgramRequirementMapping.AppliesTo's `regulatorApplies || flagApplies`.
--
-- JUDGMENT CALL: HazardMapping/RequiredSafetyModules/SopsRegistry/
-- AuditChecklistRegistry/EventRoutingRules (Models\Industries\Consumers\)
-- are NOT mirrored into SQL here -- they were out of this task's explicit
-- scope (Industry/Regulator/SafetyFlag/RegulatorCategory/
-- RegulatorRequirement/SafetyProgram only) -- so this function is built
-- directly from the tables that are in scope, using the mathematically
-- equivalent simplified rule documented above rather than re-deriving
-- those five downstream consumer catalogs.
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER FUNCTION [SafetyTaxonomy].[fn_SafetyProgram_ApplicableToIndustry]
(
    @IndustryName nvarchar(200)
)
RETURNS TABLE
AS
RETURN
(
    SELECT DISTINCT
        sp.[ProgramId],
        sp.[Code],
        sp.[Name],
        sp.[Description],
        sp.[PrimaryRegulatorId],
        sp.[RegulatorCategoryId],
        sp.[PrimaryFlagId]
    FROM [SafetyTaxonomy].[Industry] AS i
    INNER JOIN [SafetyTaxonomy].[SafetyProgram] AS sp ON 1 = 1
    WHERE i.[Name] = @IndustryName
      AND (
            EXISTS (
                SELECT 1
                FROM [SafetyTaxonomy].[IndustryRegulator] AS ir
                WHERE ir.[IndustryId] = i.[IndustryId]
                  AND ir.[RegulatorId] = sp.[PrimaryRegulatorId]
            )
            OR
            (
                sp.[PrimaryFlagId] IS NOT NULL
                AND EXISTS (
                    SELECT 1
                    FROM [SafetyTaxonomy].[IndustryFlag] AS ifl
                    WHERE ifl.[IndustryId] = i.[IndustryId]
                      AND ifl.[SafetyFlagId] = sp.[PrimaryFlagId]
                )
            )
      )
);
GO
