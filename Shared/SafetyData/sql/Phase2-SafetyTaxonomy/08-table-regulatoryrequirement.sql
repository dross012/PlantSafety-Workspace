-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           08-table-regulatoryrequirement.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [SafetyTaxonomy].[RegulatoryRequirement] --
--                  mirror of RegulatorRequirement/RegulatorRequirementCatalog
--                  (Shared.SafetyData.Abstractions\Models\Industries\
--                  RegulatorRequirement.cs / RegulatorRequirementCatalog.cs),
--                  36 rows, in RegulatorRequirementCatalog.All source-literal
--                  order (grouped by regulator; see that file's inline
--                  section comments).
--  Notes:
--    - RequirementId is an explicit, non-IDENTITY INT assigned in
--      RegulatorRequirementCatalog.All declaration order (1-36), for the
--      same determinism/reproducibility reason as the other taxonomy
--      tables; also mirrored by SafetyTaxonomy.SafetyProgram.SourceRequirementId
--      (09-table-safetyprogram.sql), since every ProgramDefinition is built
--      1:1 from exactly one RegulatorRequirement.
--    - RegulatorCategoryId is denormalized here exactly as it is in the C#
--      record (RegulatorRequirement.Category), always computed from
--      RegulatorId via RegulatorCategoryCatalog.ForRegulator and populated
--      that way in the seed script -- never hand-entered independently --
--      so the two columns can never drift apart, matching the source
--      comment's own "Category is always computed ... rather than
--      hardcoded" invariant.
--    - Read-only reference data: no TenantId, no soft-delete, no per-row
--      audit trail. Mutation outside the seed script is blocked by
--      [SafetyTaxonomy].[trg_RegulatoryRequirement_BlockMutation]
--      (40-triggers-block-mutation.sql).
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RegulatoryRequirement' AND schema_id = SCHEMA_ID('SafetyTaxonomy'))
BEGIN
    CREATE TABLE [SafetyTaxonomy].[RegulatoryRequirement]
    (
        [RequirementId]         INT             NOT NULL,
        [Name]                   NVARCHAR(200)   NOT NULL,
        [Description]            NVARCHAR(1000)  NOT NULL,
        [RegulatorId]            INT             NOT NULL,
        [RegulatorCategoryId]    INT             NOT NULL,

        CONSTRAINT PK_RegulatoryRequirement PRIMARY KEY CLUSTERED ([RequirementId]),
        CONSTRAINT UQ_RegulatoryRequirement_Name UNIQUE ([Name]),
        CONSTRAINT FK_RegulatoryRequirement_Regulator FOREIGN KEY ([RegulatorId])
            REFERENCES [SafetyTaxonomy].[Regulator] ([RegulatorId]),
        CONSTRAINT FK_RegulatoryRequirement_RegulatorCategory FOREIGN KEY ([RegulatorCategoryId])
            REFERENCES [SafetyTaxonomy].[RegulatorCategory] ([RegulatorCategoryId])
    );
END
GO

------------------------------------------------------------
-- Indexes
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RegulatoryRequirement_RegulatorId' AND object_id = OBJECT_ID('SafetyTaxonomy.RegulatoryRequirement'))
    CREATE NONCLUSTERED INDEX IX_RegulatoryRequirement_RegulatorId
        ON [SafetyTaxonomy].[RegulatoryRequirement] ([RegulatorId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RegulatoryRequirement_RegulatorCategoryId' AND object_id = OBJECT_ID('SafetyTaxonomy.RegulatoryRequirement'))
    CREATE NONCLUSTERED INDEX IX_RegulatoryRequirement_RegulatorCategoryId
        ON [SafetyTaxonomy].[RegulatoryRequirement] ([RegulatorCategoryId]);
GO
