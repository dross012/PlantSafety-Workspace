-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           05-table-industrysubindustry.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [SafetyTaxonomy].[IndustrySubIndustry] --
--                  child table normalizing IndustryDefinition.SubIndustries
--                  (Shared.SafetyData.Abstractions\Models\Industries\
--                  IndustryDefinition.cs), one row per sub-industry string.
--                  Industries whose source SubIndustries list is empty (see
--                  IndustryRegistry.cs's NoSubIndustries) simply have zero
--                  child rows here -- not a null/placeholder row.
--  Notes:
--    - IndustrySubIndustryId is a plain IDENTITY surrogate: unlike Industry/
--      Regulator/SafetyFlag/RegulatorCategory (which are enum-shaped and
--      referenced by literal id from other tables' seed scripts), no other
--      table references an individual sub-industry row, so there is no
--      determinism requirement on this id -- an IDENTITY is simplest.
--    - OrdinalPosition preserves the sub-industry's 1-based position within
--      its source C# array, since IReadOnlyList<string> order is
--      significant in the source (SubIndustries: new[] { "Bulk chemicals",
--      "solvents", ... }) and SQL provides no other ordering guarantee.
--    - Read-only reference data: no TenantId, no soft-delete, no per-row
--      audit trail. Mutation outside the seed script is blocked by
--      [SafetyTaxonomy].[trg_IndustrySubIndustry_BlockMutation]
--      (40-triggers-block-mutation.sql).
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IndustrySubIndustry' AND schema_id = SCHEMA_ID('SafetyTaxonomy'))
BEGIN
    CREATE TABLE [SafetyTaxonomy].[IndustrySubIndustry]
    (
        [IndustrySubIndustryId]   INT IDENTITY(1,1) NOT NULL,
        [IndustryId]               INT             NOT NULL,
        [SubIndustryName]          NVARCHAR(200)   NOT NULL,
        [OrdinalPosition]          INT             NOT NULL,

        CONSTRAINT PK_IndustrySubIndustry PRIMARY KEY CLUSTERED ([IndustrySubIndustryId]),
        CONSTRAINT UQ_IndustrySubIndustry_Industry_Name UNIQUE ([IndustryId], [SubIndustryName]),
        CONSTRAINT FK_IndustrySubIndustry_Industry FOREIGN KEY ([IndustryId])
            REFERENCES [SafetyTaxonomy].[Industry] ([IndustryId])
    );
END
GO

------------------------------------------------------------
-- Indexes
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_IndustrySubIndustry_IndustryId' AND object_id = OBJECT_ID('SafetyTaxonomy.IndustrySubIndustry'))
    CREATE NONCLUSTERED INDEX IX_IndustrySubIndustry_IndustryId
        ON [SafetyTaxonomy].[IndustrySubIndustry] ([IndustryId], [OrdinalPosition]);
GO
