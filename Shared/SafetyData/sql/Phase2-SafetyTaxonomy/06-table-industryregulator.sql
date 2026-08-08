-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           06-table-industryregulator.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [SafetyTaxonomy].[IndustryRegulator] --
--                  many-to-many junction table normalizing
--                  IndustryDefinition.Regulators (Shared.SafetyData.
--                  Abstractions\Models\Industries\IndustryDefinition.cs /
--                  IndustryRegistry.cs), one row per (industry, regulator)
--                  pair that appears in the source taxonomy.
--  Notes:
--    - Composite PK (IndustryId, RegulatorId): the pair is naturally unique
--      (an industry never lists the same regulator twice in its Regulators
--      array) and needs no separate surrogate key since no other table
--      references an individual junction row.
--    - OrdinalPosition preserves each regulator's 1-based position within
--      the industry's source Regulators array (order is significant in the
--      C# source, e.g. RegulatorRequirementMapping.For iterates it in
--      order).
--    - Read-only reference data: no TenantId, no soft-delete, no per-row
--      audit trail. Mutation outside the seed script is blocked by
--      [SafetyTaxonomy].[trg_IndustryRegulator_BlockMutation]
--      (40-triggers-block-mutation.sql).
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IndustryRegulator' AND schema_id = SCHEMA_ID('SafetyTaxonomy'))
BEGIN
    CREATE TABLE [SafetyTaxonomy].[IndustryRegulator]
    (
        [IndustryId]        INT   NOT NULL,
        [RegulatorId]       INT   NOT NULL,
        [OrdinalPosition]   INT   NOT NULL,

        CONSTRAINT PK_IndustryRegulator PRIMARY KEY CLUSTERED ([IndustryId], [RegulatorId]),
        CONSTRAINT FK_IndustryRegulator_Industry FOREIGN KEY ([IndustryId])
            REFERENCES [SafetyTaxonomy].[Industry] ([IndustryId]),
        CONSTRAINT FK_IndustryRegulator_Regulator FOREIGN KEY ([RegulatorId])
            REFERENCES [SafetyTaxonomy].[Regulator] ([RegulatorId])
    );
END
GO

------------------------------------------------------------
-- Indexes
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_IndustryRegulator_RegulatorId' AND object_id = OBJECT_ID('SafetyTaxonomy.IndustryRegulator'))
    CREATE NONCLUSTERED INDEX IX_IndustryRegulator_RegulatorId
        ON [SafetyTaxonomy].[IndustryRegulator] ([RegulatorId]);
GO
