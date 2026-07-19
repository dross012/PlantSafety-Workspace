-- =============================================
-- Phase 3 Architectural Remediation
-- Script:        31-reportsection-json-check.sql
-- Purpose:       Mitigate the Reporting.ReportSection JSON-polymorphism risk
--                (PayloadJson stores arbitrary per-section-type JSON that is
--                currently only validated at PDF render time in
--                Reporting.Infrastructure). Add a CHECK constraint requiring
--                well-formed JSON as a first-line, zero-guessing defense.
--                This does NOT fully solve the polymorphism risk (a
--                well-formed-but-wrong-shape payload for a given SectionType
--                will still only be caught at render time) -- per user's
--                stated preference, that is accepted as a follow-on concern,
--                not something to guess a fix for here.
--
-- Verified live 2026-07-18 against (local)\SQLEXPRESS, PlantSafety.ReportingDB:
--   - Reporting.ReportSection columns: ReportSectionId (uniqueidentifier, PK),
--     ReportDocumentId (uniqueidentifier), SectionType (nvarchar(400), NOT
--     NULL), Title (nvarchar(400)), SortOrder (int), PayloadJson
--     (nvarchar(MAX), NULLABLE), CreatedUtc, CreatedBy, IsDeleted.
--   - No existing CHECK constraints on this table.
--   - Table confirmed 0 rows.
--
-- SectionType CHECK constraint: INTENTIONALLY SKIPPED.
--   Per the task's no-guessing rule ("use real enum values, don't invent
--   them; skip if you can't find a real source"): this repository has NO
--   Reporting.Domain project/source code (confirmed via search of
--   PlantSafety.Domain and the full solution tree -- no ReportSection.cs,
--   ReportType.cs, or SectionType enum exists anywhere in the built
--   codebase). The only "Reporting" code that actually exists in this repo
--   is PlantSafety.Infrastructure/Analytics/Reporting/ReportingDapperRepository.cs,
--   which has no relation to a SectionType enum. The section-type/report-type
--   list in the Reporting Rules & Design Manual (memory:
--   reporting_rules_design.md) is design documentation for a conceptual
--   Reporting.Domain that was never actually built in this solution -- it is
--   not "the Reporting Domain-layer source" the task asked for, so treating
--   its class names (TitlePageSection, ExecutiveSummarySection, etc.) as the
--   authoritative finite value set would be guessing, not sourcing. Flagged
--   here rather than silently invented; add this CHECK once/if a real
--   Reporting.Domain SectionType enum is committed to the codebase.
--
-- PayloadJson CHECK constraint: PayloadJson is NULLABLE, so the constraint
-- must explicitly allow NULL (a NULL PayloadJson is valid -- not every
-- section carries a JSON payload) and only require well-formed JSON when a
-- value is present.
--
-- Idempotent: guarded by sys.check_constraints existence check; safe to
-- re-run. Table confirmed 0 rows before and after.
-- =============================================

USE [PlantSafety.ReportingDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE name = N'CK_Reporting_ReportSection_PayloadJson_IsJson'
      AND parent_object_id = OBJECT_ID(N'[Reporting].[ReportSection]')
)
BEGIN
    ALTER TABLE [Reporting].[ReportSection]
        ADD CONSTRAINT CK_Reporting_ReportSection_PayloadJson_IsJson
        CHECK (PayloadJson IS NULL OR ISJSON(PayloadJson) = 1);
END;
GO
