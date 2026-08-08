-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           02-table-regulatorcategory.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [SafetyTaxonomy].[RegulatorCategory] --
--                  mirror of the 9-value RegulatorCategory enum
--                  (Shared.SafetyData.Abstractions\Models\Industries\
--                  RegulatorCategory.cs). Every one of the 15 Regulator
--                  values maps onto exactly one of these 9 categories via
--                  RegulatorCategoryCatalog.ForRegulator (see
--                  03-table-regulator.sql's RegulatorCategoryId column).
--  Notes:
--    - RegulatorCategoryId is an explicit, non-IDENTITY INT assigned in
--      RegulatorCategory.cs declaration order (1-9), for the same
--      determinism/reproducibility reason as SafetyFlag.SafetyFlagId.
--    - Read-only reference data: no TenantId, no soft-delete, no per-row
--      audit trail. Mutation outside the seed script is blocked by
--      [SafetyTaxonomy].[trg_RegulatorCategory_BlockMutation]
--      (40-triggers-block-mutation.sql).
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RegulatorCategory' AND schema_id = SCHEMA_ID('SafetyTaxonomy'))
BEGIN
    CREATE TABLE [SafetyTaxonomy].[RegulatorCategory]
    (
        [RegulatorCategoryId]   INT             NOT NULL,
        [CategoryName]          NVARCHAR(50)    NOT NULL,

        CONSTRAINT PK_RegulatorCategory PRIMARY KEY CLUSTERED ([RegulatorCategoryId]),
        CONSTRAINT UQ_RegulatorCategory_CategoryName UNIQUE ([CategoryName])
    );
END
GO
