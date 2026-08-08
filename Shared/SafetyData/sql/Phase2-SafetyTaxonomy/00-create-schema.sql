-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           00-create-schema.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Creates the [SafetyTaxonomy] schema -- the dedicated,
--                  separately-named SQL mirror of the deterministic,
--                  in-memory-only C# reference taxonomy in
--                  Shared.SafetyData.Abstractions.Models.Industries
--                  (IndustryRegistry, Regulator, SafetyFlag,
--                  RegulatorCategoryCatalog, RegulatorRequirementCatalog,
--                  ProgramCatalog).
--
--  NAMING / COLLISION NOTE: [SafetyTaxonomy] is deliberately NOT [dbo] and
--  deliberately NOT reused from any existing PlantSafety schema, because
--  PlantSafety already has real, tenant-owned, mutable modules that would
--  otherwise collide by name:
--    - [SafetyPrograms].[SafetyProgram] (PlantSafety.Infrastructure\
--      SafetyPrograms\Sql\Tables\SafetyProgram.sql) -- a tenant-scoped,
--      mutable aggregate root with its own usp_SafetyProgram_Create/Update/
--      GetById/List/Count procedures. [SafetyTaxonomy].[SafetyProgram] is a
--      completely different, read-only reference concept (one row per
--      RegulatorRequirementCatalog entry) and lives in a different schema so
--      the two can never be confused or joined by accident.
--    - [Compliance].[RegulatoryRequirements] / [Compliance].[RegulatoryCalendar]
--      (PlantSafety.Infrastructure\Compliance\...) -- existing regulatory-
--      compliance modules, untouched by this schema.
--
--  GOVERNANCE NOTE (applies to every object under this schema): every table
--  here is deterministic, seeded, read-only reference/master data mirroring
--  a compiled C# static catalog -- the C# catalogs remain the single source
--  of truth. There is therefore no TenantId, no soft-delete (IsDeleted), and
--  no per-row audit trail (CreatedBy/UpdatedBy) on these tables the way a
--  mutable PlantSafety aggregate would have -- those conventions exist to
--  track *tenant-owned mutable* data, and this data is neither tenant-owned
--  nor (outside the seed scripts) mutable at all. Mutation protection is
--  instead enforced schema-wide via the triggers in 40-triggers-block-mutation.sql.
--
--  DATABASE PLACEMENT: [PlantSafety.CoreDB], the same physical database
--  already used by Shared.SafetyData's Phase1-StoredProcedures scripts
--  (01-usp-get-hazard-summary.sql, 02-usp-get-corrective-actions.sql,
--  03-usp-get-safety-compliance.sql), per the canonical database-per-solution
--  inventory in sql-database-skill/references/00-canonical-templates.md.
--
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'SafetyTaxonomy')
BEGIN
    EXEC('CREATE SCHEMA [SafetyTaxonomy] AUTHORIZATION dbo;');
END
GO
