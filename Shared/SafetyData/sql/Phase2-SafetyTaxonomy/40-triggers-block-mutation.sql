-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           40-triggers-block-mutation.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Blocks ad-hoc INSERT/UPDATE/DELETE against every table in
--                  the [SafetyTaxonomy] schema outside of the seed scripts'
--                  own execution context.
--
--  RATIONALE (why this exists at all): every table in [SafetyTaxonomy] is a
--  deterministic SQL mirror of a compiled, static C# catalog in
--  Shared.SafetyData.Abstractions.Models.Industries (IndustryRegistry,
--  Regulator, SafetyFlag, RegulatorCategoryCatalog,
--  RegulatorRequirementCatalog, ProgramCatalog). The C# catalogs are, and
--  remain, the single authoritative source of truth for this taxonomy --
--  this SQL mirror exists purely so PlantSafety/reporting SQL callers can
--  query/join the taxonomy relationally without round-tripping through the
--  application layer. If application code (or an ad-hoc SSMS session) were
--  able to INSERT/UPDATE/DELETE rows here directly, the SQL mirror would
--  silently desynchronize from the real source of truth with no compiler,
--  test, or migration ever catching the drift -- a stale/wrong regulator
--  code or an orphaned industry-flag row could sit undetected indefinitely.
--  Blocking mutation entirely (except from the seed scripts themselves,
--  10-seed-safetyflag.sql .. 18-seed-safetyprogram.sql, which are the only
--  code paths that re-derive this data from the C# source) is the only way
--  to guarantee that invariant holds.
--
--  MECHANISM: a straightforward RAISERROR-and-ROLLBACK AFTER trigger, per
--  explicit task instruction (no established PlantSafety pattern was found
--  for "block all DML including INSERT, except from a designated seed
--  context" specifically -- the one existing reference-table protection
--  precedent in this repo, [Compliance].[trg_NIOSHHazardChangeLog_NoUpdateDelete]
--  (PlantSafety.Infrastructure\Compliance\Sql\Tables\
--  NIOSHHazardChangeLog.sql), is an INSTEAD OF UPDATE, DELETE trigger for an
--  APPEND-ONLY table -- i.e. it deliberately still allows INSERT. That
--  precedent does not fit here because every [SafetyTaxonomy] table must
--  reject INSERT too, except from its own seed script). Each trigger below
--  checks SESSION_CONTEXT(N'SafetyTaxonomySeedContext') (set to 1 by each
--  seed script for the duration of its own MERGE statements, and reset to 0
--  immediately afterward -- see e.g. 10-seed-safetyflag.sql) and returns
--  immediately (allowing the DML) when that flag is set; otherwise it
--  raises an error and rolls back the triggering statement's transaction.
--
--  SHARED APPROACH: SQL Server triggers are per-table objects (no single
--  trigger object can span multiple tables), so this file creates one
--  trigger per [SafetyTaxonomy] table using an identical pattern/rationale
--  -- functionally "a single shared approach" applied uniformly, per the
--  task's own phrasing, even though each is a distinct CREATE TRIGGER
--  statement. Applied to ALL NINE tables in the schema (Industry,
--  IndustrySubIndustry, Regulator, RegulatorCategory, SafetyFlag,
--  IndustryRegulator, IndustryFlag, RegulatoryRequirement, SafetyProgram),
--  not just the three named as the minimum in the task instructions
--  (Regulator, RegulatorCategory, SafetyProgram) -- every one of these
--  tables is equally seeded/read-only reference data with the identical
--  desync risk, so scoping protection down to only three would leave six
--  tables silently exposed to the exact same problem this file exists to
--  prevent.
--
--  Idempotent: DROP+CREATE per trigger. Safe to re-run. Additive-only
--  evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

------------------------------------------------------------
-- SafetyTaxonomy.SafetyFlag
------------------------------------------------------------
IF OBJECT_ID('SafetyTaxonomy.trg_SafetyFlag_BlockMutation', 'TR') IS NOT NULL
    DROP TRIGGER [SafetyTaxonomy].[trg_SafetyFlag_BlockMutation];
GO

CREATE TRIGGER [SafetyTaxonomy].[trg_SafetyFlag_BlockMutation]
ON [SafetyTaxonomy].[SafetyFlag]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF ISNULL(CAST(SESSION_CONTEXT(N'SafetyTaxonomySeedContext') AS bit), 0) = 1
        RETURN;

    RAISERROR(N'SafetyTaxonomy.SafetyFlag is read-only reference data mirrored from Shared.SafetyData.Abstractions.Models.Industries.SafetyFlag (C#). Direct INSERT/UPDATE/DELETE is blocked -- change the C# catalog and re-run its seed script instead.', 16, 1);
    ROLLBACK TRANSACTION;
END
GO

------------------------------------------------------------
-- SafetyTaxonomy.RegulatorCategory
------------------------------------------------------------
IF OBJECT_ID('SafetyTaxonomy.trg_RegulatorCategory_BlockMutation', 'TR') IS NOT NULL
    DROP TRIGGER [SafetyTaxonomy].[trg_RegulatorCategory_BlockMutation];
GO

CREATE TRIGGER [SafetyTaxonomy].[trg_RegulatorCategory_BlockMutation]
ON [SafetyTaxonomy].[RegulatorCategory]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF ISNULL(CAST(SESSION_CONTEXT(N'SafetyTaxonomySeedContext') AS bit), 0) = 1
        RETURN;

    RAISERROR(N'SafetyTaxonomy.RegulatorCategory is read-only reference data mirrored from Shared.SafetyData.Abstractions.Models.Industries.RegulatorCategory (C#). Direct INSERT/UPDATE/DELETE is blocked -- change the C# catalog and re-run its seed script instead.', 16, 1);
    ROLLBACK TRANSACTION;
END
GO

------------------------------------------------------------
-- SafetyTaxonomy.Regulator
------------------------------------------------------------
IF OBJECT_ID('SafetyTaxonomy.trg_Regulator_BlockMutation', 'TR') IS NOT NULL
    DROP TRIGGER [SafetyTaxonomy].[trg_Regulator_BlockMutation];
GO

CREATE TRIGGER [SafetyTaxonomy].[trg_Regulator_BlockMutation]
ON [SafetyTaxonomy].[Regulator]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF ISNULL(CAST(SESSION_CONTEXT(N'SafetyTaxonomySeedContext') AS bit), 0) = 1
        RETURN;

    RAISERROR(N'SafetyTaxonomy.Regulator is read-only reference data mirrored from Shared.SafetyData.Abstractions.Models.Industries.Regulator (C#). Direct INSERT/UPDATE/DELETE is blocked -- change the C# catalog and re-run its seed script instead.', 16, 1);
    ROLLBACK TRANSACTION;
END
GO

------------------------------------------------------------
-- SafetyTaxonomy.Industry
------------------------------------------------------------
IF OBJECT_ID('SafetyTaxonomy.trg_Industry_BlockMutation', 'TR') IS NOT NULL
    DROP TRIGGER [SafetyTaxonomy].[trg_Industry_BlockMutation];
GO

CREATE TRIGGER [SafetyTaxonomy].[trg_Industry_BlockMutation]
ON [SafetyTaxonomy].[Industry]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF ISNULL(CAST(SESSION_CONTEXT(N'SafetyTaxonomySeedContext') AS bit), 0) = 1
        RETURN;

    RAISERROR(N'SafetyTaxonomy.Industry is read-only reference data mirrored from Shared.SafetyData.Abstractions.Models.Industries.IndustryRegistry (C#). Direct INSERT/UPDATE/DELETE is blocked -- change the C# catalog and re-run its seed script instead.', 16, 1);
    ROLLBACK TRANSACTION;
END
GO

------------------------------------------------------------
-- SafetyTaxonomy.IndustrySubIndustry
------------------------------------------------------------
IF OBJECT_ID('SafetyTaxonomy.trg_IndustrySubIndustry_BlockMutation', 'TR') IS NOT NULL
    DROP TRIGGER [SafetyTaxonomy].[trg_IndustrySubIndustry_BlockMutation];
GO

CREATE TRIGGER [SafetyTaxonomy].[trg_IndustrySubIndustry_BlockMutation]
ON [SafetyTaxonomy].[IndustrySubIndustry]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF ISNULL(CAST(SESSION_CONTEXT(N'SafetyTaxonomySeedContext') AS bit), 0) = 1
        RETURN;

    RAISERROR(N'SafetyTaxonomy.IndustrySubIndustry is read-only reference data mirrored from Shared.SafetyData.Abstractions.Models.Industries.IndustryRegistry (C#). Direct INSERT/UPDATE/DELETE is blocked -- change the C# catalog and re-run its seed script instead.', 16, 1);
    ROLLBACK TRANSACTION;
END
GO

------------------------------------------------------------
-- SafetyTaxonomy.IndustryRegulator
------------------------------------------------------------
IF OBJECT_ID('SafetyTaxonomy.trg_IndustryRegulator_BlockMutation', 'TR') IS NOT NULL
    DROP TRIGGER [SafetyTaxonomy].[trg_IndustryRegulator_BlockMutation];
GO

CREATE TRIGGER [SafetyTaxonomy].[trg_IndustryRegulator_BlockMutation]
ON [SafetyTaxonomy].[IndustryRegulator]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF ISNULL(CAST(SESSION_CONTEXT(N'SafetyTaxonomySeedContext') AS bit), 0) = 1
        RETURN;

    RAISERROR(N'SafetyTaxonomy.IndustryRegulator is read-only reference data mirrored from Shared.SafetyData.Abstractions.Models.Industries.IndustryRegistry (C#). Direct INSERT/UPDATE/DELETE is blocked -- change the C# catalog and re-run its seed script instead.', 16, 1);
    ROLLBACK TRANSACTION;
END
GO

------------------------------------------------------------
-- SafetyTaxonomy.IndustryFlag
------------------------------------------------------------
IF OBJECT_ID('SafetyTaxonomy.trg_IndustryFlag_BlockMutation', 'TR') IS NOT NULL
    DROP TRIGGER [SafetyTaxonomy].[trg_IndustryFlag_BlockMutation];
GO

CREATE TRIGGER [SafetyTaxonomy].[trg_IndustryFlag_BlockMutation]
ON [SafetyTaxonomy].[IndustryFlag]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF ISNULL(CAST(SESSION_CONTEXT(N'SafetyTaxonomySeedContext') AS bit), 0) = 1
        RETURN;

    RAISERROR(N'SafetyTaxonomy.IndustryFlag is read-only reference data mirrored from Shared.SafetyData.Abstractions.Models.Industries.IndustryRegistry (C#). Direct INSERT/UPDATE/DELETE is blocked -- change the C# catalog and re-run its seed script instead.', 16, 1);
    ROLLBACK TRANSACTION;
END
GO

------------------------------------------------------------
-- SafetyTaxonomy.RegulatoryRequirement
------------------------------------------------------------
IF OBJECT_ID('SafetyTaxonomy.trg_RegulatoryRequirement_BlockMutation', 'TR') IS NOT NULL
    DROP TRIGGER [SafetyTaxonomy].[trg_RegulatoryRequirement_BlockMutation];
GO

CREATE TRIGGER [SafetyTaxonomy].[trg_RegulatoryRequirement_BlockMutation]
ON [SafetyTaxonomy].[RegulatoryRequirement]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF ISNULL(CAST(SESSION_CONTEXT(N'SafetyTaxonomySeedContext') AS bit), 0) = 1
        RETURN;

    RAISERROR(N'SafetyTaxonomy.RegulatoryRequirement is read-only reference data mirrored from Shared.SafetyData.Abstractions.Models.Industries.RegulatorRequirementCatalog (C#). Direct INSERT/UPDATE/DELETE is blocked -- change the C# catalog and re-run its seed script instead.', 16, 1);
    ROLLBACK TRANSACTION;
END
GO

------------------------------------------------------------
-- SafetyTaxonomy.SafetyProgram
------------------------------------------------------------
IF OBJECT_ID('SafetyTaxonomy.trg_SafetyProgram_BlockMutation', 'TR') IS NOT NULL
    DROP TRIGGER [SafetyTaxonomy].[trg_SafetyProgram_BlockMutation];
GO

CREATE TRIGGER [SafetyTaxonomy].[trg_SafetyProgram_BlockMutation]
ON [SafetyTaxonomy].[SafetyProgram]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF ISNULL(CAST(SESSION_CONTEXT(N'SafetyTaxonomySeedContext') AS bit), 0) = 1
        RETURN;

    RAISERROR(N'SafetyTaxonomy.SafetyProgram is read-only reference data mirrored from Shared.SafetyData.Abstractions.Models.Industries.ProgramCatalog (C#). Direct INSERT/UPDATE/DELETE is blocked -- this table is DISTINCT from PlantSafety''s tenant-scoped, mutable [SafetyPrograms].[SafetyProgram] -- change the C# catalog and re-run its seed script instead.', 16, 1);
    ROLLBACK TRANSACTION;
END
GO
