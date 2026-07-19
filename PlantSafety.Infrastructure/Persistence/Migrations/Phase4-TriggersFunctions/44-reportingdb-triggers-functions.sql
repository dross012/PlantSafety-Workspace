USE [PlantSafety.ReportingDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 4 Triggers & Functions
-- Script:        44-reportingdb-triggers-functions.sql
-- Author:        Trigger & Function Engineer (unattended agent run)
--
-- SECTION 1 -- Update-audit triggers
--   Reporting.ReportDefinition and Reporting.ReportType carry
--   ConcurrencyToken + UpdatedUtc/UpdatedBy; AFTER UPDATE triggers force
--   UpdatedUtc = SYSUTCDATETIME() on every update -- see the in-body comment
--   for why UpdatedBy is left to the app/SP layer. ReportDocument,
--   ReportMetadata, and ReportSection carry neither ConcurrencyToken nor
--   UpdatedUtc/UpdatedBy (generated-artifact / append-style records tied to a
--   ReportDefinition run) and are intentionally excluded.
--
-- SECTION 2 -- Soft-delete triggers: DELIBERATELY NOT CREATED
--   Per enterprise_data_database_architecture_master.md / sql-database-skill:
--   triggers must be "AFTER only (never INSTEAD OF)" and restricted to
--   "audit/error/summary-refresh orchestration only, never business logic".
--   Converting DELETE to soft-delete needs an INSTEAD OF trigger, which is
--   explicitly forbidden; this stays an application/SP-layer responsibility.
--
-- SECTION 3 -- Functions: NONE CREATED
--   No TenantId column exists on any ReportingDB table, so fn_IsWithinTenant
--   is not applicable. No computed business formula analogous to
--   RiskManagement.RiskScore was found in the reporting domain source (per
--   reporting_rules_design.md, PDF generation logic lives in
--   Infrastructure/C# only, never T-SQL). No scalar/inline-TVF functions are
--   warranted for this database.
--
-- Idempotent:    CREATE OR ALTER throughout (SQL Server 2025 / compat >=150).
-- =============================================

-- ===================== SECTION 1: Update-audit triggers =====================

CREATE OR ALTER TRIGGER [Reporting].[trg_ReportDefinition_UpdateAudit]
ON [Reporting].[ReportDefinition]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Reporting].[ReportDefinition] AS t
    INNER JOIN inserted AS i
        ON t.[ReportDefinitionId] = i.[ReportDefinitionId];
END
GO

CREATE OR ALTER TRIGGER [Reporting].[trg_ReportType_UpdateAudit]
ON [Reporting].[ReportType]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Reporting].[ReportType] AS t
    INNER JOIN inserted AS i
        ON t.[ReportTypeId] = i.[ReportTypeId];
END
GO


