-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           04-table-industry.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [SafetyTaxonomy].[Industry] -- mirror of
--                  IndustryDefinition/IndustryRegistry.All (Shared.SafetyData.
--                  Abstractions\Models\Industries\IndustryDefinition.cs /
--                  IndustryRegistry.cs), 40 rows. SubIndustries, Regulators,
--                  and Flags are normalized out to their own child/junction
--                  tables (05, 06, 07) since IndustryDefinition holds each of
--                  those as a list/bitmask, not a scalar column.
--  Notes:
--    - IndustryId is an explicit, non-IDENTITY INT assigned in
--      IndustryRegistry.All declaration/taxonomy order (1-40), for the same
--      determinism/reproducibility reason as the other taxonomy tables.
--    - TopLevelCategory is a free NVARCHAR column (not a lookup table) --
--      the 8 top-level categories are C# `const string` literals in
--      IndustryRegistry.cs, not an enum, so there is no corresponding
--      catalog type to mirror as a separate table; a CHECK constraint pins
--      it to the exact 8 values documented in IndustryRegistry.cs's header.
--    - Read-only reference data: no TenantId, no soft-delete, no per-row
--      audit trail. Mutation outside the seed script is blocked by
--      [SafetyTaxonomy].[trg_Industry_BlockMutation]
--      (40-triggers-block-mutation.sql).
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Industry' AND schema_id = SCHEMA_ID('SafetyTaxonomy'))
BEGIN
    CREATE TABLE [SafetyTaxonomy].[Industry]
    (
        [IndustryId]         INT             NOT NULL,
        [Name]                NVARCHAR(200)   NOT NULL,
        [TopLevelCategory]    NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_Industry PRIMARY KEY CLUSTERED ([IndustryId]),
        CONSTRAINT UQ_Industry_Name UNIQUE ([Name]),
        CONSTRAINT CK_Industry_TopLevelCategory CHECK ([TopLevelCategory] IN (
            N'Core Manufacturing',
            N'Logistics & Transport',
            N'High Fire/Explosion Risk',
            N'High Chemical Hazard',
            N'Heavy Machinery / Industrial',
            N'Water/Marine Interface',
            N'High Environmental Compliance',
            N'Specialized'
        ))
    );
END
GO

------------------------------------------------------------
-- Indexes
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Industry_TopLevelCategory' AND object_id = OBJECT_ID('SafetyTaxonomy.Industry'))
    CREATE NONCLUSTERED INDEX IX_Industry_TopLevelCategory
        ON [SafetyTaxonomy].[Industry] ([TopLevelCategory]);
GO
