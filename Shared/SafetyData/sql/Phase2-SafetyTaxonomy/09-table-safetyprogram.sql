-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           09-table-safetyprogram.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [SafetyTaxonomy].[SafetyProgram] -- mirror
--                  of ProgramDefinition/ProgramCatalog (Shared.SafetyData.
--                  Abstractions\Models\Industries\ProgramDefinition.cs /
--                  ProgramCatalog.cs), 36 rows, built 1:1 from
--                  [SafetyTaxonomy].[RegulatoryRequirement], in
--                  ProgramCatalog.All order (Regulator enum declaration
--                  order, i.e. Enum.GetValues<Regulator>().SelectMany(...)).
--
--  NAMING / COLLISION NOTE: this is a DIFFERENT table from PlantSafety's
--  existing, tenant-scoped, mutable [SafetyPrograms].[SafetyProgram]
--  (PlantSafety.Infrastructure\SafetyPrograms\Sql\Tables\SafetyProgram.sql).
--  This table lives in the [SafetyTaxonomy] schema (not [SafetyPrograms]),
--  has an unrelated column shape (no TenantId/SiteId/Status/EnrolledCount/
--  Goals/RowVersion), and represents a completely different concept: a
--  fixed, deterministic catalog of the 36 regulatory Safety Programs a
--  facility might need to implement, not a specific tenant's in-flight
--  program instance. See 00-create-schema.sql's header for the full
--  rationale.
--
--  Notes:
--    - ProgramId is an explicit, non-IDENTITY INT assigned in
--      ProgramCatalog.All order (1-36), for the same determinism/
--      reproducibility reason as the other taxonomy tables.
--    - Code mirrors ProgramDefinition.Code verbatim (e.g. "PROG-OSHA-01",
--      "PROG-EPA_RCRA-01") -- built from the Regulator enum's raw C# member
--      name (EnumName, e.g. "EPA_RCRA"), NOT the hyphenated Code() display
--      form (e.g. "EPA-RCRA") -- see ProgramCatalog.cs's
--      $"PROG-{regulator}-{index + 1:D2}" interpolation, which calls
--      Regulator.ToString(), not RegulatorExtensions.Code().
--    - SourceRequirementId is a mandatory 1:1 FK back to
--      [SafetyTaxonomy].[RegulatoryRequirement], mirroring "every
--      ProgramDefinition instance is built 1:1 from
--      RegulatorRequirementCatalog.All" (ProgramCatalog.cs header) -- Name
--      and Description are still duplicated onto this table (rather than
--      requiring a join) because ProgramDefinition.Name/Description are
--      themselves plain copies of RegulatorRequirement.Name/Description in
--      the C# source, so duplicating here is a direct mirror, not new
--      denormalization.
--    - PrimaryRegulatorId / RegulatorCategoryId are denormalized exactly as
--      in the C# record (ProgramDefinition.PrimaryRegulator/.Category).
--    - PrimaryFlagId is NULLable, mirroring ProgramDefinition.PrimaryFlag's
--      own nullability (SafetyFlag?) -- null exactly for the 3 FDA-sourced
--      programs (cGMP, Sanitary Facility Operations, Quality Systems
--      Regulation), which have no hazard-flag correlate in this system (see
--      ProgramCatalog.cs's PrimaryFlagFor rationale).
--    - Read-only reference data: no TenantId, no soft-delete, no per-row
--      audit trail. Mutation outside the seed script is blocked by
--      [SafetyTaxonomy].[trg_SafetyProgram_BlockMutation]
--      (40-triggers-block-mutation.sql).
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SafetyProgram' AND schema_id = SCHEMA_ID('SafetyTaxonomy'))
BEGIN
    CREATE TABLE [SafetyTaxonomy].[SafetyProgram]
    (
        [ProgramId]              INT             NOT NULL,
        [Code]                    NVARCHAR(20)    NOT NULL,
        [Name]                    NVARCHAR(200)   NOT NULL,
        [Description]             NVARCHAR(1000)  NOT NULL,
        [SourceRequirementId]     INT             NOT NULL,
        [PrimaryRegulatorId]      INT             NOT NULL,
        [RegulatorCategoryId]     INT             NOT NULL,
        [PrimaryFlagId]           INT             NULL,

        CONSTRAINT PK_SafetyProgram PRIMARY KEY CLUSTERED ([ProgramId]),
        CONSTRAINT UQ_SafetyProgram_Code UNIQUE ([Code]),
        CONSTRAINT UQ_SafetyProgram_SourceRequirementId UNIQUE ([SourceRequirementId]),
        CONSTRAINT FK_SafetyProgram_RegulatoryRequirement FOREIGN KEY ([SourceRequirementId])
            REFERENCES [SafetyTaxonomy].[RegulatoryRequirement] ([RequirementId]),
        CONSTRAINT FK_SafetyProgram_Regulator FOREIGN KEY ([PrimaryRegulatorId])
            REFERENCES [SafetyTaxonomy].[Regulator] ([RegulatorId]),
        CONSTRAINT FK_SafetyProgram_RegulatorCategory FOREIGN KEY ([RegulatorCategoryId])
            REFERENCES [SafetyTaxonomy].[RegulatorCategory] ([RegulatorCategoryId]),
        CONSTRAINT FK_SafetyProgram_SafetyFlag FOREIGN KEY ([PrimaryFlagId])
            REFERENCES [SafetyTaxonomy].[SafetyFlag] ([SafetyFlagId])
    );
END
GO

------------------------------------------------------------
-- Indexes
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SafetyProgram_PrimaryRegulatorId' AND object_id = OBJECT_ID('SafetyTaxonomy.SafetyProgram'))
    CREATE NONCLUSTERED INDEX IX_SafetyProgram_PrimaryRegulatorId
        ON [SafetyTaxonomy].[SafetyProgram] ([PrimaryRegulatorId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SafetyProgram_PrimaryFlagId' AND object_id = OBJECT_ID('SafetyTaxonomy.SafetyProgram'))
    CREATE NONCLUSTERED INDEX IX_SafetyProgram_PrimaryFlagId
        ON [SafetyTaxonomy].[SafetyProgram] ([PrimaryFlagId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SafetyProgram_RegulatorCategoryId' AND object_id = OBJECT_ID('SafetyTaxonomy.SafetyProgram'))
    CREATE NONCLUSTERED INDEX IX_SafetyProgram_RegulatorCategoryId
        ON [SafetyTaxonomy].[SafetyProgram] ([RegulatorCategoryId]);
GO
