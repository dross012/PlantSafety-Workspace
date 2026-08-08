-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           03-table-regulator.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [SafetyTaxonomy].[Regulator] -- mirror of
--                  the 15-value Regulator enum (Shared.SafetyData.
--                  Abstractions\Models\Industries\Regulator.cs), including
--                  the exact, verbatim regulator Code (e.g. "EPA-RCRA")
--                  recovered by RegulatorExtensions.Code() in the C# source.
--  Notes:
--    - RegulatorId is an explicit, non-IDENTITY INT assigned in Regulator.cs
--      declaration order (1-15), for the same determinism/reproducibility
--      reason as SafetyFlag.SafetyFlagId.
--    - RegulatorCategoryId is a mandatory FK to
--      [SafetyTaxonomy].[RegulatorCategory], mirroring
--      RegulatorCategoryCatalog.ForRegulator's total (every Regulator maps
--      onto exactly one category) mapping.
--    - EnumName carries the raw C# enum member name (e.g. "EPA_RCRA",
--      underscored) as distinct from Code (e.g. "EPA-RCRA", hyphenated) --
--      both are needed verbatim: EnumName is what ProgramCatalog.cs's
--      Code-building interpolation ($"PROG-{regulator}-{index}") actually
--      uses (see SafetyProgram.Code in 09-table-safetyprogram.sql), while
--      Code is the human-facing, source-taxonomy-verbatim string.
--    - Read-only reference data: no TenantId, no soft-delete, no per-row
--      audit trail. Mutation outside the seed script is blocked by
--      [SafetyTaxonomy].[trg_Regulator_BlockMutation]
--      (40-triggers-block-mutation.sql).
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Regulator' AND schema_id = SCHEMA_ID('SafetyTaxonomy'))
BEGIN
    CREATE TABLE [SafetyTaxonomy].[Regulator]
    (
        [RegulatorId]           INT             NOT NULL,
        [EnumName]               NVARCHAR(20)    NOT NULL,
        [Code]                   NVARCHAR(20)    NOT NULL,
        [Description]            NVARCHAR(400)   NOT NULL,
        [RegulatorCategoryId]    INT             NOT NULL,

        CONSTRAINT PK_Regulator PRIMARY KEY CLUSTERED ([RegulatorId]),
        CONSTRAINT UQ_Regulator_EnumName UNIQUE ([EnumName]),
        CONSTRAINT UQ_Regulator_Code UNIQUE ([Code]),
        CONSTRAINT FK_Regulator_RegulatorCategory FOREIGN KEY ([RegulatorCategoryId])
            REFERENCES [SafetyTaxonomy].[RegulatorCategory] ([RegulatorCategoryId])
    );
END
GO

------------------------------------------------------------
-- Indexes
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Regulator_RegulatorCategoryId' AND object_id = OBJECT_ID('SafetyTaxonomy.Regulator'))
    CREATE NONCLUSTERED INDEX IX_Regulator_RegulatorCategoryId
        ON [SafetyTaxonomy].[Regulator] ([RegulatorCategoryId]);
GO
