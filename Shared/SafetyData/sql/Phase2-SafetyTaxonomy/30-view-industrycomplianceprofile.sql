-- =============================================
-- Shared.SafetyData -- Phase2-SafetyTaxonomy
-- Script:        30-view-industrycomplianceprofile.sql
-- Purpose:       Creates SafetyTaxonomy.vw_IndustryComplianceProfile against
--                [PlantSafety.CoreDB] -- a single denormalized read model
--                joining [SafetyTaxonomy].[Industry] +
--                [SafetyTaxonomy].[IndustryRegulator] +
--                [SafetyTaxonomy].[RegulatoryRequirement] +
--                [SafetyTaxonomy].[SafetyProgram], mirroring what
--                RegulatorRequirementMapping (Shared.SafetyData.
--                Abstractions\Models\Industries\
--                RegulatorRequirementMapping.cs) and ProgramCatalog
--                (ProgramCatalog.cs, built 1:1 from
--                RegulatorRequirementCatalog.All) already compute in memory
--                for every industry, joined together: for a given industry,
--                every regulatory requirement its regulators impose, and
--                the one Safety Program each requirement operationalizes.
--
-- JUDGMENT CALL (view scope vs. usp_GetProgramsByIndustryName /
-- fn_SafetyProgram_ApplicableToIndustry): this view mirrors the
-- REGULATOR-driven path only (one row per Industry x IndustryRegulator x
-- RegulatoryRequirement x its 1:1 SafetyProgram) -- exactly the four
-- objects named in this view's own task description. It intentionally does
-- NOT also fold in the FLAG-driven program-applicability path (programs
-- that apply via PrimaryFlag membership in the industry's Flags, e.g. an
-- OSHA PIT/Machine-Guarding program applying to an industry that carries
-- Machinery but not through a regulator match) -- that path already has its
-- own dedicated, purpose-built objects
-- (19-fn-safetyprogram-applicabletoindustry.sql /
-- 24-usp-get-programs-by-industry-name.sql), and folding it into this join
-- would require an OR/UNION across a structurally different join shape
-- (Industry x IndustryFlag x SafetyFlag, no RegulatoryRequirement
-- involved), which would stop this from being "a single denormalized read
-- model" over the four named tables and start being a duplicate of the
-- function. A future SafetyTaxonomy.vw_IndustryApplicablePrograms could
-- wrap the function if a single-object read model over the full applies-via-
-- either-path rule is needed.
--
-- Determinism / soft-delete: no ORDER BY (views must not embed one per
-- validate-view-determinism; callers order the result), explicit column
-- list, no nondeterministic functions. No IsDeleted filter -- unlike the
-- canonical view template, none of these four tables carries a soft-delete
-- column (see each table's own DDL header: this is immutable, seeded
-- reference/master data, never soft-deleted).
--
-- SAFETY: read-only view. Must never be defined WITH SCHEMABINDING over a
-- mutable table, and must never itself be the target of an INSTEAD OF
-- INSERT/UPDATE/DELETE trigger that bypasses
-- [SafetyTaxonomy].[trg_*_BlockMutation] on its base tables.
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [SafetyTaxonomy].[vw_IndustryComplianceProfile]
AS
SELECT
    i.[IndustryId],
    i.[Name]                          AS [IndustryName],
    i.[TopLevelCategory],
    r.[RegulatorId],
    r.[EnumName]                      AS [RegulatorEnumName],
    r.[Code]                          AS [RegulatorCode],
    ir.[OrdinalPosition]              AS [IndustryRegulatorOrdinalPosition],
    rr.[RequirementId],
    rr.[Name]                         AS [RequirementName],
    rr.[Description]                  AS [RequirementDescription],
    rr.[RegulatorCategoryId],
    sp.[ProgramId],
    sp.[Code]                         AS [ProgramCode],
    sp.[Name]                         AS [ProgramName],
    sp.[Description]                  AS [ProgramDescription],
    sp.[PrimaryFlagId]                AS [ProgramPrimaryFlagId]
FROM [SafetyTaxonomy].[Industry] AS i
INNER JOIN [SafetyTaxonomy].[IndustryRegulator] AS ir ON ir.[IndustryId] = i.[IndustryId]
INNER JOIN [SafetyTaxonomy].[Regulator] AS r ON r.[RegulatorId] = ir.[RegulatorId]
INNER JOIN [SafetyTaxonomy].[RegulatoryRequirement] AS rr ON rr.[RegulatorId] = r.[RegulatorId]
INNER JOIN [SafetyTaxonomy].[SafetyProgram] AS sp ON sp.[SourceRequirementId] = rr.[RequirementId];
GO
