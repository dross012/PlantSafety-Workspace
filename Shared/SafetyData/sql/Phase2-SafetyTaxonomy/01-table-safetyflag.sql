-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           01-table-safetyflag.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for [SafetyTaxonomy].[SafetyFlag] -- the
--                  normalized, one-row-per-bit mirror of the [Flags] enum
--                  Shared.SafetyData.Abstractions.Models.Industries.SafetyFlag
--                  (Shared.SafetyData.Abstractions\Models\Industries\
--                  SafetyFlag.cs). SafetyFlag is a bitwise-combinable
--                  [Flags] enum in C# so a single IndustryDefinition.Flags
--                  value can carry any subset; SQL has no native bitmask
--                  type suited to relational joins/filters, so this table
--                  normalizes each of the 10 individual flag bits to its own
--                  row (SafetyTaxonomy.IndustryFlag is the corresponding
--                  Industry <-> SafetyFlag junction table).
--  Notes:
--    - SafetyFlagId is an explicit, non-IDENTITY INT assigned in
--      SafetyFlag.cs declaration order (1-10), not an IDENTITY surrogate --
--      this keeps the id deterministic and reproducible across every
--      environment/re-seed, which matters because other tables' seed
--      scripts (04, 07, 09) reference these ids by literal value.
--    - BitValue mirrors the C# [Flags] numeric value (1 << 0 .. 1 << 9)
--      verbatim, for traceability back to SafetyFlag.cs; SQL itself never
--      needs to recompose the bitmask, only join on SafetyFlagId.
--    - Read-only reference data: no TenantId, no soft-delete, no per-row
--      audit trail (see 00-create-schema.sql header for rationale). Mutation
--      outside the seed script is blocked by
--      [SafetyTaxonomy].[trg_SafetyFlag_BlockMutation]
--      (40-triggers-block-mutation.sql).
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SafetyFlag' AND schema_id = SCHEMA_ID('SafetyTaxonomy'))
BEGIN
    CREATE TABLE [SafetyTaxonomy].[SafetyFlag]
    (
        [SafetyFlagId]   INT             NOT NULL,
        [FlagName]       NVARCHAR(50)    NOT NULL,
        [BitValue]       INT             NOT NULL,
        [Description]    NVARCHAR(400)   NOT NULL,

        CONSTRAINT PK_SafetyFlag PRIMARY KEY CLUSTERED ([SafetyFlagId]),
        CONSTRAINT UQ_SafetyFlag_FlagName UNIQUE ([FlagName]),
        CONSTRAINT UQ_SafetyFlag_BitValue UNIQUE ([BitValue]),
        CONSTRAINT CK_SafetyFlag_BitValue CHECK ([BitValue] > 0)
    );
END
GO
