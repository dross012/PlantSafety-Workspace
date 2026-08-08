-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           07-table-industryflag.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [SafetyTaxonomy].[IndustryFlag] --
--                  many-to-many junction table normalizing
--                  IndustryDefinition.Flags (Shared.SafetyData.Abstractions
--                  \Models\Industries\IndustryDefinition.cs /
--                  IndustryRegistry.cs), one row per single-bit SafetyFlag
--                  set in an industry's combined Flags bitmask (equivalent
--                  to SafetyFlagCatalog.Decompose(industry.Flags) --
--                  Shared.SafetyData.Abstractions\Models\Industries\
--                  SafetyFlagCatalog.cs).
--  Notes:
--    - Composite PK (IndustryId, SafetyFlagId): a flag bit is either present
--      or absent on a given industry, so the pair is naturally unique; no
--      separate surrogate key needed.
--    - Read-only reference data: no TenantId, no soft-delete, no per-row
--      audit trail. Mutation outside the seed script is blocked by
--      [SafetyTaxonomy].[trg_IndustryFlag_BlockMutation]
--      (40-triggers-block-mutation.sql).
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IndustryFlag' AND schema_id = SCHEMA_ID('SafetyTaxonomy'))
BEGIN
    CREATE TABLE [SafetyTaxonomy].[IndustryFlag]
    (
        [IndustryId]     INT   NOT NULL,
        [SafetyFlagId]   INT   NOT NULL,

        CONSTRAINT PK_IndustryFlag PRIMARY KEY CLUSTERED ([IndustryId], [SafetyFlagId]),
        CONSTRAINT FK_IndustryFlag_Industry FOREIGN KEY ([IndustryId])
            REFERENCES [SafetyTaxonomy].[Industry] ([IndustryId]),
        CONSTRAINT FK_IndustryFlag_SafetyFlag FOREIGN KEY ([SafetyFlagId])
            REFERENCES [SafetyTaxonomy].[SafetyFlag] ([SafetyFlagId])
    );
END
GO

------------------------------------------------------------
-- Indexes
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_IndustryFlag_SafetyFlagId' AND object_id = OBJECT_ID('SafetyTaxonomy.IndustryFlag'))
    CREATE NONCLUSTERED INDEX IX_IndustryFlag_SafetyFlagId
        ON [SafetyTaxonomy].[IndustryFlag] ([SafetyFlagId]);
GO
